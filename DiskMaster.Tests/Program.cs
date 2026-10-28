using DiskMaster.Models;
using DiskMaster.Services;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

// Tests de sécurité sans bibliothèque externe. Tous les fichiers sont isolés dans ce dossier.
string testRoot = Path.Combine(AppContext.BaseDirectory, "fixtures", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(testRoot);
int passed = 0;
int failed = 0;
var progress = new InlineProgress();

async Task Check(string name, Func<Task> test)
{
    try
    {
        await test();
        passed++;
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"FAIL {name}\n{ex}");
    }
}

void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

(CleanupService Service, CleanupCategory Category) Setup(string name, CleanupKind kind = CleanupKind.TemporaryFiles)
{
    string root = Path.Combine(testRoot, name);
    Directory.CreateDirectory(root);
    var category = new CleanupCategory(kind, name, "test", root);
    return (new CleanupService([category], _ => { }), category);
}

string AddFile(string root, string name, bool old = true)
{
    string path = Path.Combine(root, name);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllText(path, "fixture");
    if (old) File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-10));
    return path;
}

await Check("Analyse : âge, sous-dossiers et taille", async () =>
{
    var (service, category) = Setup("scan");
    AddFile(category.Root, "old.tmp");
    AddFile(category.Root, "sub/old.tmp");
    AddFile(category.Root, "recent.tmp", false);
    var result = (await service.ScanAsync(progress, default)).Single();
    Assert(result.Files.Count == 2 && result.SizeBytes == 14, "Sélection incorrecte.");
});

await Check("Nettoyage limité à l’instantané et aux fichiers inchangés", async () =>
{
    var (service, category) = Setup("snapshot");
    string deleted = AddFile(category.Root, "old.tmp");
    string changed = AddFile(category.Root, "changed.tmp");
    var scan = await service.ScanAsync(progress, default);
    File.AppendAllText(changed, "changed");
    string added = AddFile(category.Root, "added-after-scan.tmp");
    var result = await service.CleanAsync(scan, progress, default);
    Assert(result.Deleted == 1 && result.Skipped == 1 && result.DeletedBytes == 7, "Bilan incorrect.");
    Assert(!File.Exists(deleted) && File.Exists(changed) && File.Exists(added), "Limite de l’instantané violée.");
    Assert(Directory.Exists(category.Root), "Dossier racine supprimé.");
});

await Check("Refus d’un fichier extérieur au dossier autorisé", async () =>
{
    var (service, category) = Setup("boundary");
    string outside = AddFile(testRoot, "outside.tmp");
    var info = new FileInfo(outside);
    var forged = new ScanResult(category, [new(outside, info.Length, info.LastWriteTimeUtc)], 0, true);
    var result = await service.CleanAsync([forged], progress, default);
    Assert(result.Deleted == 0 && result.Skipped == 1 && File.Exists(outside), "Fichier extérieur supprimé.");
    Assert(!CleanupService.IsSafePath(category.Root, category.Root + "-other/file.tmp"), "Préfixe voisin accepté.");
});

await Check("Annulation avant suppression", async () =>
{
    var (service, category) = Setup("cancel");
    string path = AddFile(category.Root, "old.tmp");
    var scan = await service.ScanAsync(progress, default);
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();
    var result = await service.CleanAsync(scan, progress, cancellation.Token);
    Assert(result.Canceled && result.Deleted == 0 && File.Exists(path), "Annulation ignorée.");
});

await Check("Annulation de l’analyse", async () =>
{
    var (service, _) = Setup("cancel-scan");
    using var cancellation = new CancellationTokenSource();
    cancellation.Cancel();
    try { await service.ScanAsync(progress, cancellation.Token); }
    catch (OperationCanceledException) { return; }
    throw new InvalidOperationException("Analyse non annulée.");
});

await Check("Rapports de plantage : uniquement .dmp", async () =>
{
    var (service, category) = Setup("dumps", CleanupKind.CrashDumps);
    AddFile(category.Root, "report.dmp");
    AddFile(category.Root, "keep.txt");
    var scan = (await service.ScanAsync(progress, default)).Single();
    Assert(scan.Files.Count == 1 && scan.Files[0].Path.EndsWith(".dmp"), "Extension non autorisée.");
});

await Check("Fichier verrouillé : ignoré sans interrompre le nettoyage", async () =>
{
    var (service, category) = Setup("locked");
    string path = AddFile(category.Root, "old.tmp");
    var scan = await service.ScanAsync(progress, default);
    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
    var result = await service.CleanAsync(scan, progress, default);
    Assert(result.Deleted == 0 && result.Skipped == 1 && File.Exists(path), "Fichier verrouillé mal traité.");
});

await Check("Fichier disparu après analyse", async () =>
{
    var (service, category) = Setup("missing-file");
    string path = AddFile(category.Root, "old.tmp");
    var scan = await service.ScanAsync(progress, default);
    File.Delete(path);
    var result = await service.CleanAsync(scan, progress, default);
    Assert(result.Deleted == 0 && result.Skipped == 1, "Fichier absent mal traité.");
});

await Check("Fichier protégé après analyse", async () =>
{
    var (service, category) = Setup("readonly");
    string path = AddFile(category.Root, "old.tmp");
    var scan = await service.ScanAsync(progress, default);
    File.SetAttributes(path, FileAttributes.ReadOnly);
    try
    {
        var result = await service.CleanAsync(scan, progress, default);
        Assert(result.Deleted == 0 && File.Exists(path), "Protection ignorée.");
    }
    finally { File.SetAttributes(path, FileAttributes.Normal); }
});

await Check("Dossier absent", async () =>
{
    var (service, category) = Setup("missing-directory");
    Directory.Delete(category.Root);
    var result = (await service.ScanAsync(progress, default)).Single();
    Assert(!result.Exists && result.Files.Count == 0, "Dossier absent mal traité.");
});

await Check("Catégorie étrangère refusée", async () =>
{
    var (service, _) = Setup("catalog");
    var foreign = new CleanupCategory(CleanupKind.TemporaryFiles, "foreign", "", testRoot);
    try { await service.CleanAsync([new(foreign, [], 0, true)], progress, default); }
    catch (InvalidOperationException) { return; }
    throw new InvalidOperationException("Catégorie étrangère acceptée.");
});

await Check("Doublons : un seul fichier comptabilisé", async () =>
{
    var (service, category) = Setup("duplicates");
    AddFile(category.Root, "old.tmp");
    var scan = (await service.ScanAsync(progress, default)).Single();
    var duplicate = scan with { Files = [scan.Files[0], scan.Files[0]] };
    var result = await service.CleanAsync([duplicate], progress, default);
    Assert(result.Deleted == 1 && result.DeletedBytes == 7 && result.Skipped == 1, "Double comptabilisation.");
});

await Check("Jonction : cible exclue de l’analyse et de la suppression", async () =>
{
    var (service, category) = Setup("junction");
    string target = Path.Combine(testRoot, "junction-target");
    string outside = AddFile(target, "keep.tmp");
    string link = Path.Combine(category.Root, "redirect");
    var start = new System.Diagnostics.ProcessStartInfo("powershell.exe")
    {
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    start.ArgumentList.Add("-NoProfile");
    start.ArgumentList.Add("-NonInteractive");
    start.ArgumentList.Add("-Command");
    start.ArgumentList.Add($"New-Item -ItemType Junction -Path '{link.Replace("'", "''")}' -Target '{target.Replace("'", "''")}' -ErrorAction Stop | Out-Null");
    using var process = System.Diagnostics.Process.Start(start)!;
    await process.WaitForExitAsync();
    Assert(process.ExitCode == 0, "Création de la jonction impossible : " + await process.StandardError.ReadToEndAsync());
    try
    {
        var scan = (await service.ScanAsync(progress, default)).Single();
        Assert(scan.Files.Count == 0 && scan.Skipped > 0, "Jonction suivie pendant l’analyse.");
        var info = new FileInfo(outside);
        var forged = new ScanResult(category, [new(Path.Combine(link, "keep.tmp"), info.Length, info.LastWriteTimeUtc)], 0, true);
        var result = await service.CleanAsync([forged], progress, default);
        Assert(result.Deleted == 0 && File.Exists(outside), "Cible de jonction supprimée.");
    }
    finally { Directory.Delete(link); }
});

await Check("Notifications : propriété calculée et changement de sélection", () =>
{
    var (_, category) = Setup("notifications");
    var vm = new DiskMaster.ViewModels.CategoryViewModel(category);
    var notifications = new List<string?>();
    vm.PropertyChanged += (_, e) => notifications.Add(e.PropertyName);
    vm.IsSelected = false;
    vm.Result = new ScanResult(category, [], 0, true);
    Assert(notifications.Contains("IsSelected") && notifications.Contains("Size") && notifications.Contains("Status"),
        "Notifications manquantes.");
    return Task.CompletedTask;
});

await Check("Annulation pendant suppression : bilan partiel exact", async () =>
{
    var (service, category) = Setup("mid-cancel");
    for (int i = 0; i < 30; i++) AddFile(category.Root, $"{i}.tmp");
    var scan = await service.ScanAsync(progress, default);
    using var cancellation = new CancellationTokenSource();
    var callback = new CallbackProgress(value => { if (value.Completed == 25) cancellation.Cancel(); });
    var result = await service.CleanAsync(scan, callback, cancellation.Token);
    Assert(result is { Canceled: true, Deleted: 25, DeletedBytes: 175 }, "Bilan partiel incorrect.");
    Assert(Directory.EnumerateFiles(category.Root).Count() == 5, "Nombre de fichiers restants incorrect.");
});

await Check("Même taille, date différente : fichier ignoré", async () =>
{
    var (service, category) = Setup("same-size");
    string path = AddFile(category.Root, "old.tmp");
    var scan = await service.ScanAsync(progress, default);
    File.WriteAllText(path, "changed");
    File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-9));
    var result = await service.CleanAsync(scan, progress, default);
    Assert(result.Deleted == 0 && result.Skipped == 1 && File.Exists(path), "Modification de même taille ignorée.");
});

await Check("Catalogue invalide : refus avant toute suppression", async () =>
{
    var (service, category) = Setup("preflight");
    string path = AddFile(category.Root, "old.tmp");
    var scan = (await service.ScanAsync(progress, default)).Single();
    var foreign = new CleanupCategory(CleanupKind.TemporaryFiles, "foreign", "", testRoot);
    try { await service.CleanAsync([scan, new(foreign, [], 0, true)], progress, default); }
    catch (InvalidOperationException) { Assert(File.Exists(path), "Suppression avant validation complète."); return; }
    throw new InvalidOperationException("Catégorie étrangère acceptée.");
});

await Check("Accès refusé à un sous-dossier : résultat partiel conservé", async () =>
{
    var (service, category) = Setup("acl");
    AddFile(category.Root, "allowed.tmp");
    AddFile(category.Root, "denied/keep.tmp");
    var directory = new DirectoryInfo(Path.Combine(category.Root, "denied"));
    byte[] original = directory.GetAccessControl(AccessControlSections.Access).GetSecurityDescriptorBinaryForm();
    var restricted = new DirectorySecurity();
    restricted.SetSecurityDescriptorBinaryForm(original, AccessControlSections.Access);
    var sid = WindowsIdentity.GetCurrent().User!;
    restricted.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.ListDirectory,
        InheritanceFlags.None, PropagationFlags.None, AccessControlType.Deny));
    try
    {
        directory.SetAccessControl(restricted);
        var result = (await service.ScanAsync(progress, default)).Single();
        Assert(result.Files.Count == 1 && result.Skipped > 0, "Résultat partiel incorrect en cas d’accès refusé.");
    }
    finally
    {
        var restore = new DirectorySecurity();
        restore.SetSecurityDescriptorBinaryForm(original, AccessControlSections.Access);
        directory.SetAccessControl(restore);
    }
});

await AdditionalTests.RunAsync(testRoot, Check);

// Aucun effacement récursif : seuls les fichiers de cette fixture sont retirés.
foreach (string file in Directory.EnumerateFiles(testRoot, "*", SearchOption.AllDirectories)) File.Delete(file);
foreach (string directory in Directory.EnumerateDirectories(testRoot, "*", SearchOption.AllDirectories)
    .OrderByDescending(path => path.Length)) Directory.Delete(directory);
Directory.Delete(testRoot);
Console.WriteLine($"{passed} tests réussis.");
if (failed > 0) Console.Error.WriteLine($"{failed} test(s) échoué(s).");
Environment.ExitCode = failed > 0 ? 1 : 0;

sealed class InlineProgress : IProgress<CleanupProgress>
{
    public void Report(CleanupProgress value) { }
}

sealed class CallbackProgress(Action<CleanupProgress> callback) : IProgress<CleanupProgress>
{
    public void Report(CleanupProgress value) => callback(value);
}
