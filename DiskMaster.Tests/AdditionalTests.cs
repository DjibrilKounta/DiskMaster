using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using DiskMaster;
using DiskMaster.Infrastructure;
using DiskMaster.Models;
using DiskMaster.Services;
using DiskMaster.ViewModels;

internal static class AdditionalTests
{
    public static async Task RunAsync(string root, Func<string, Func<Task>, Task> check)
    {
        await check("ViewModel : analyse, sélection et refus de confirmation", async () =>
        {
            var cleanup = new FakeCleanup(); var data = new FakeData(root); var desktop = new FakeDesktop();
            var vm = new MainViewModel(cleanup, data, desktop);
            Require(!vm.CleanCommand.CanExecute(null), "Nettoyage disponible avant analyse.");
            await vm.ScanCommand.ExecuteAsync();
            Require(vm.SelectedFiles == 1 && vm.CleanCommand.CanExecute(null), "Analyse non appliquée.");
            vm.Categories[0].IsSelected = false;
            Require(!vm.CleanCommand.CanExecute(null), "Sélection ignorée.");
            vm.Categories[0].IsSelected = true;
            await vm.CleanCommand.ExecuteAsync();
            Require(desktop.Confirmations == 1 && cleanup.CleanCalls == 0 && data.History is null,
                "Suppression ou historique malgré un refus.");
            Require(vm.CleanCommand.CanExecute(null), "Analyse invalidée malgré le refus.");
        });

        await check("ViewModel : nettoyage partiel, historique et invalidation", async () =>
        {
            var cleanup = new FakeCleanup(); var data = new FakeData(root); var desktop = new FakeDesktop { Consent = true };
            var vm = new MainViewModel(cleanup, data, desktop);
            await vm.ScanCommand.ExecuteAsync();
            await vm.CleanCommand.ExecuteAsync();
            Require(data.History is { Canceled: true, Deleted: 1, DeletedBytes: 7 }, "Bilan partiel perdu.");
            Require(vm.Summary.Contains("interrompue") && !vm.IsBusy && !vm.CleanCommand.CanExecute(null),
                "État final incorrect.");
            Require(vm.Categories.All(c => c.Result is null), "Instantané réutilisable après nettoyage.");
        });

        await check("ViewModel : annulation et disponibilité des commandes", async () =>
        {
            var cleanup = new FakeCleanup { BlockScan = true };
            var vm = new MainViewModel(cleanup, new FakeData(root), new FakeDesktop());
            Task operation = vm.ScanCommand.ExecuteAsync();
            Require(vm.IsBusy && !vm.ScanCommand.CanExecute(null) && vm.CancelCommand.CanExecute(null), "État occupé incorrect.");
            vm.CancelCommand.Execute(null);
            await operation;
            Require(!vm.IsBusy && vm.Status.Contains("annulée") && !vm.CleanCommand.CanExecute(null), "Annulation incorrecte.");
        });

        await check("ViewModel : erreur du moteur signalée et état rétabli", async () =>
        {
            var cleanup = new FakeCleanup { FailClean = true }; var desktop = new FakeDesktop { Consent = true };
            var data = new FakeData(root); var vm = new MainViewModel(cleanup, data, desktop);
            await vm.ScanCommand.ExecuteAsync(); await vm.CleanCommand.ExecuteAsync();
            Require(!vm.IsBusy && desktop.Errors == 1 && data.Messages.Count == 1 && data.History is null,
                "Erreur masquée ou état incorrect.");
            Require(!vm.CleanCommand.CanExecute(null), "Analyse réutilisable après erreur.");
        });

        await check("ViewModel : échec de sauvegarde sans perdre le bilan", async () =>
        {
            var data = new FakeData(root) { FailSave = true };
            var vm = new MainViewModel(new FakeCleanup(), data, new FakeDesktop { Consent = true });
            await vm.ScanCommand.ExecuteAsync(); await vm.CleanCommand.ExecuteAsync();
            Require(vm.Summary.Contains("1 fichier(s) supprimé(s)") && vm.Summary.Contains("n’a pas pu"), "Bilan perdu.");
        });

        await check("Progression retardée : ne remplace pas le résultat final", () =>
        {
            var previous = SynchronizationContext.Current; var context = new QueuedContext();
            var cleanup = new FakeCleanup();
            try
            {
                SynchronizationContext.SetSynchronizationContext(context);
                var vm = new MainViewModel(cleanup, new FakeData(root), new FakeDesktop());
                vm.ScanCommand.ExecuteAsync().GetAwaiter().GetResult();
                string final = vm.Status;
                cleanup.Progress!.Report(new("Ancien message", 0, 3));
                context.Drain();
                Require(vm.Status == final && vm.Progress == 100, "Progression périmée appliquée.");
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
            return Task.CompletedTask;
        });

        await check("Commande asynchrone : réentrance interdite et erreurs relayées", async () =>
        {
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            int calls = 0, errors = 0;
            var command = new AsyncCommand(async () => { calls++; await gate.Task; throw new IOException("test"); }, _ => errors++);
            Task first = command.ExecuteAsync();
            await command.ExecuteAsync();
            Require(calls == 1 && !command.CanExecute(null), "Réentrance autorisée.");
            gate.SetResult(); await first;
            Require(errors == 1 && command.CanExecute(null), "Commande bloquée après erreur.");
        });

        await check("Historique : sauvegarde et relecture dans un dossier isolé", () =>
        {
            var data = new UserDataService(Path.Combine(root, "history-roundtrip"));
            var expected = new CleanupHistory(DateTimeOffset.UtcNow, 2, 1, 64, true);
            data.Save(expected);
            Require(data.Load() == expected, "Historique différent après relecture.");
            Require(!Directory.EnumerateFiles(data.Folder, "*.tmp").Any(), "Fichier temporaire résiduel.");
            return Task.CompletedTask;
        });

        await check("Historique corrompu : récupération sans crash", () =>
        {
            var data = new UserDataService(Path.Combine(root, "history-corrupt"));
            Directory.CreateDirectory(data.Folder);
            File.WriteAllText(Path.Combine(data.Folder, "history.json"), "{broken");
            Require(data.Load() is null && File.Exists(Path.Combine(data.Folder, "diagnostic.log")), "Corruption non signalée.");
            return Task.CompletedTask;
        });

        await check("Diagnostics : rotation du journal", () =>
        {
            var data = new UserDataService(Path.Combine(root, "rotation"));
            Directory.CreateDirectory(data.Folder);
            File.WriteAllText(Path.Combine(data.Folder, "diagnostic.log"), new string('x', 1_000_001));
            data.Log("nouveau message");
            Require(File.Exists(Path.Combine(data.Folder, "diagnostic.log.old")), "Rotation absente.");
            Require(File.ReadAllText(Path.Combine(data.Folder, "diagnostic.log")).Contains("nouveau message"), "Message perdu.");
            return Task.CompletedTask;
        });

        await check("WPF : ressources, bindings et mise en page avec faux services", () => SmokeWpfAsync(root));
    }

    private static Task SmokeWpfAsync(string root)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            Application? app = null;
            var writer = new StringWriter();
            var listener = new TextWriterTraceListener(writer);
            var source = PresentationTraceSources.DataBindingSource;
            var previousLevel = source.Switch.Level;
            source.Switch.Level = SourceLevels.Error;
            source.Listeners.Add(listener);
            try
            {
                app = new App(); ((App)app).InitializeComponent();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var vm = new MainViewModel(new FakeCleanup(), new FakeData(root), new FakeDesktop());
                var window = new MainWindow(vm) { ShowInTaskbar = false };
                window.Show();
                vm.ScanCommand.ExecuteAsync().GetAwaiter().GetResult();
                var frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(() => frame.Continue = false));
                Dispatcher.PushFrame(frame);
                window.Width = 920; window.Height = 760; window.UpdateLayout();
                Require(window.ActualWidth >= 920 && window.ActualHeight >= 760, "Dimensions invalides.");
                listener.Flush();
                Require(string.IsNullOrWhiteSpace(writer.ToString()), "Erreur de binding : " + writer);
                window.Close();
                completion.SetResult();
            }
            catch (Exception ex) { completion.SetException(ex); }
            finally
            {
                app?.Shutdown();
                source.Listeners.Remove(listener); source.Switch.Level = previousLevel;
                listener.Dispose(); writer.Dispose();
            }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task.WaitAsync(TimeSpan.FromSeconds(30));
    }

    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }

    private sealed class FakeCleanup : ICleanupService
    {
        public IReadOnlyList<CleanupCategory> Categories { get; } =
            [new(CleanupKind.TemporaryFiles, "Fixture", "Aucun fichier réel", "C:\\DiskMaster-Test-Not-Used")];
        public bool BlockScan { get; init; }
        public bool FailClean { get; init; }
        public int CleanCalls { get; private set; }
        public IProgress<CleanupProgress>? Progress { get; private set; }
        public async Task<IReadOnlyList<ScanResult>> ScanAsync(IProgress<CleanupProgress> progress, CancellationToken token)
        {
            Progress = progress;
            if (BlockScan) await Task.Delay(Timeout.Infinite, token);
            return [new(Categories[0], [new("C:\\fixture.tmp", 7, DateTime.UtcNow.AddDays(-10))], 0, true)];
        }
        public Task<CleanupResult> CleanAsync(IReadOnlyList<ScanResult> selection, IProgress<CleanupProgress> progress, CancellationToken token)
        {
            CleanCalls++;
            return FailClean ? Task.FromException<CleanupResult>(new IOException("Erreur simulée"))
                : Task.FromResult(new CleanupResult(1, 0, 7, true));
        }
    }
    private sealed class FakeData(string folder) : IUserDataService
    {
        public string Folder => folder;
        public bool FailSave { get; init; }
        public CleanupHistory? History { get; private set; }
        public List<string> Messages { get; } = [];
        public CleanupHistory? Load() => null;
        public void Save(CleanupHistory history)
        { if (FailSave) throw new IOException("Sauvegarde simulée"); History = history; }
        public void Log(string message) => Messages.Add(message);
    }
    private sealed class FakeDesktop : IDesktopService
    {
        public bool Consent { get; init; }
        public int Confirmations { get; private set; }
        public int Errors { get; private set; }
        public bool Confirm(string message) { Confirmations++; return Consent; }
        public void ShowError(string message) => Errors++;
        public void OpenFolder(string path) { }
        public void OpenStorageSettings() { }
    }
    private sealed class QueuedContext : SynchronizationContext
    {
        private readonly Queue<Action> _callbacks = new();
        public override void Post(SendOrPostCallback callback, object? state) => _callbacks.Enqueue(() => callback(state));
        public void Drain() { while (_callbacks.TryDequeue(out var callback)) callback(); }
    }
}
