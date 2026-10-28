using DiskMaster.Infrastructure;
using DiskMaster.Models;

namespace DiskMaster.ViewModels;

public sealed class CategoryViewModel(CleanupCategory category) : ObservableObject
{
    private bool _isSelected = true;
    private ScanResult? _result;
    public CleanupCategory Category { get; } = category;
    public string Name => Category.Name;
    public string Description => Category.Description;
    public string Root => Category.Root;
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
    public ScanResult? Result
    {
        get => _result;
        set
        {
            if (!Set(ref _result, value)) return;
            Notify(nameof(Size));
            Notify(nameof(Status));
        }
    }
    public string Size => Result is null ? "—" : ByteSize.Format(Result.SizeBytes);
    public string Status => Result switch
    {
        null => "À analyser",
        { Exists: false } => "Dossier absent",
        { Skipped: > 0 } r => $"{r.Files.Count} fichier(s) • {r.Skipped} élément(s) inaccessible(s) ou exclu(s)",
        var r => $"{r.Files.Count} fichier(s) éligible(s)"
    };
}
