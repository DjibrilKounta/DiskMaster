using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using DiskMaster.Infrastructure;
using DiskMaster.Models;
using DiskMaster.Services;

namespace DiskMaster.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly ICleanupService _cleanup;
    private readonly IUserDataService _data;
    private readonly IDesktopService _desktop;
    private CancellationTokenSource? _cancellation;
    private bool _isBusy;
    private bool _hasScan;
    private string _status = "Prêt. Lancez une analyse pour examiner les fichiers éligibles.";
    private string _lastCleanup;
    private string _summary = "Aucun nettoyage effectué pendant cette session.";
    private double _progress;
    private int _operationId;
    private CategoryViewModel? _selectedCategory;

    public MainViewModel(ICleanupService cleanup, IUserDataService data, IDesktopService desktop)
    {
        _cleanup = cleanup;
        _data = data;
        _desktop = desktop;
        Categories = new(cleanup.Categories.Select(category => new CategoryViewModel(category)));
        ScanCommand = new(ScanAsync, HandleError, () => !IsBusy);
        CleanCommand = new(CleanAsync, HandleError, () => !IsBusy && _hasScan && SelectedFiles > 0);
        CancelCommand = new(() => { _cancellation?.Cancel(); Status = "Annulation demandée…"; }, () => IsBusy);
        OpenFolderCommand = new(() => _desktop.OpenFolder(SelectedCategory!.Root), () =>
            !IsBusy && SelectedCategory is not null && Directory.Exists(SelectedCategory.Root), HandleError);
        StorageCommand = new(() => { _desktop.OpenStorageSettings(); return Task.CompletedTask; }, HandleError, () => !IsBusy);
        DiagnosticsCommand = new(() => { Directory.CreateDirectory(_data.Folder); _desktop.OpenFolder(_data.Folder); },
            () => !IsBusy, HandleError);
        foreach (var category in Categories) category.PropertyChanged += CategoryChanged;
        _selectedCategory = Categories.FirstOrDefault();
        var history = data.Load();
        _lastCleanup = history is null ? "Aucun historique de nettoyage" :
            $"Dernière opération : {history.Date.LocalDateTime:g} • {ByteSize.Format(history.DeletedBytes)} supprimés"
            + (history.Canceled ? " • interrompue" : "");
    }

    public ObservableCollection<CategoryViewModel> Categories { get; }
    public string ApplicationVersion => "DiskMaster " +
        (typeof(MainViewModel).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion.Split('+')[0] ?? "version inconnue");
    public ObservableCollection<FileCandidate> PreviewFiles { get; } = new();
    public AsyncCommand ScanCommand { get; }
    public AsyncCommand CleanCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand OpenFolderCommand { get; }
    public AsyncCommand StorageCommand { get; }
    public RelayCommand DiagnosticsCommand { get; }
    public bool IsBusy { get => _isBusy; private set { if (Set(ref _isBusy, value)) { Notify(nameof(IsIdle)); RefreshCommands(); } } }
    public bool IsIdle => !IsBusy;
    public string Status { get => _status; private set => Set(ref _status, value); }
    public string LastCleanup { get => _lastCleanup; private set => Set(ref _lastCleanup, value); }
    public string Summary { get => _summary; private set => Set(ref _summary, value); }
    public double Progress { get => _progress; private set => Set(ref _progress, value); }
    public string SelectedSize => ByteSize.Format(Categories.Where(c => c.IsSelected).Sum(c => c.Result?.SizeBytes ?? 0));
    public int SelectedFiles => Categories.Where(c => c.IsSelected).Sum(c => c.Result?.Files.Count ?? 0);
    public string SelectionLabel => $"{SelectedFiles} fichier(s) sélectionné(s) • estimation issue de l’analyse";
    public string PreviewLabel => SelectedCategory?.Result is { } r
        ? $"Aperçu : {Math.Min(200, r.Files.Count)} / {r.Files.Count} fichier(s) — {SelectedCategory.Name}"
        : "Sélectionnez une catégorie après l’analyse pour consulter ses fichiers.";
    public CategoryViewModel? SelectedCategory
    {
        get => _selectedCategory;
        set { if (Set(ref _selectedCategory, value)) { UpdatePreview(); OpenFolderCommand.Refresh(); } }
    }

    private void CategoryChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CategoryViewModel.Result) or nameof(CategoryViewModel.IsSelected))
        {
            Notify(nameof(SelectedSize)); Notify(nameof(SelectedFiles)); Notify(nameof(SelectionLabel));
            CleanCommand.Refresh();
            if (sender == SelectedCategory) UpdatePreview();
        }
    }

    private void UpdatePreview()
    {
        PreviewFiles.Clear();
        foreach (var file in SelectedCategory?.Result?.Files.Take(200) ?? []) PreviewFiles.Add(file);
        Notify(nameof(PreviewLabel));
    }

    private IProgress<CleanupProgress> CreateProgress()
    {
        int operationId = _operationId;
        return new Progress<CleanupProgress>(p =>
        {
            // Une notification retardée ne doit pas écraser le résultat final.
            if (!IsBusy || operationId != _operationId) return;
            Status = p.Message;
            Progress = p.Total == 0 ? 0 : 100.0 * p.Completed / p.Total;
        });
    }

    private async Task ScanAsync()
    {
        BeginOperation();
        _hasScan = false;
        foreach (var category in Categories) category.Result = null;
        try
        {
            var results = await _cleanup.ScanAsync(CreateProgress(), _cancellation!.Token);
            foreach (var result in results) Categories.Single(c => c.Category == result.Category).Result = result;
            _hasScan = true;
            Progress = 100;
            int skipped = results.Sum(r => r.Skipped);
            Status = skipped > 0 ? $"Analyse partielle : {skipped} élément(s) inaccessible(s) ou exclu(s)." : "Analyse terminée. Vérifiez votre sélection avant de nettoyer.";
        }
        catch (OperationCanceledException) { Status = "Analyse annulée. Relancez une analyse pour nettoyer."; }
        finally { EndOperation(); }
    }

    private async Task CleanAsync()
    {
        var selection = Categories.Where(c => c.IsSelected && c.Result is not null).Select(c => c.Result!).ToArray();
        string names = string.Join(Environment.NewLine, selection.Select(r => $"• {r.Category.Name}"));
        if (!_desktop.Confirm($"Supprimer définitivement {SelectedFiles} fichier(s), soit environ {SelectedSize} ?\n\n{names}\n\nSeuls les fichiers analysés, inchangés et âgés d’au moins 7 jours seront traités.\nCette opération ne peut pas être annulée après suppression.")) return;
        BeginOperation();
        try
        {
            var result = await _cleanup.CleanAsync(selection, CreateProgress(), _cancellation!.Token);
            Summary = $"{result.Deleted} fichier(s) supprimé(s) • {ByteSize.Format(result.DeletedBytes)} supprimés • {result.Skipped} ignoré(s)."
                + (result.Canceled ? " Opération interrompue : les suppressions déjà effectuées sont conservées." : "");
            Status = "Opération terminée. Relancez l’analyse pour actualiser les tailles.";
            var history = new CleanupHistory(DateTimeOffset.Now, result.Deleted, result.Skipped, result.DeletedBytes, result.Canceled);
            LastCleanup = $"Dernière opération : {history.Date.LocalDateTime:g} • {ByteSize.Format(result.DeletedBytes)} supprimés";
            try { _data.Save(history); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { _data.Log(ex.ToString()); Summary += " L’historique n’a pas pu être enregistré."; }
        }
        finally
        {
            _hasScan = false;
            foreach (var category in Categories) category.Result = null;
            EndOperation();
        }
    }

    private void BeginOperation()
    {
        _operationId++;
        _cancellation = new();
        Progress = 0;
        IsBusy = true;
    }
    private void EndOperation()
    {
        _cancellation?.Dispose(); _cancellation = null;
        IsBusy = false;
    }
    private void RefreshCommands()
    {
        ScanCommand.Refresh(); CleanCommand.Refresh(); CancelCommand.Refresh();
        OpenFolderCommand.Refresh(); StorageCommand.Refresh(); DiagnosticsCommand.Refresh();
    }
    private void HandleError(Exception ex)
    {
        _data.Log(ex.ToString());
        Status = "L’opération a rencontré une erreur. Consultez le journal de diagnostic.";
        _desktop.ShowError($"Impossible de terminer l’opération.\n\n{ex.Message}");
    }
}
