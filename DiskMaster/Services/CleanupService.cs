using System.IO;
using System.Security;
using DiskMaster.Models;

namespace DiskMaster.Services;

public sealed class CleanupService : ICleanupService
{
    private readonly IReadOnlyList<CleanupCategory> _categories;
    private readonly Action<string> _log;
    public IReadOnlyList<CleanupCategory> Categories => _categories;

    public CleanupService(Action<string> log) : this(CleanupCatalog.Create(), log) { }

    // Interne : les tests emploient des dossiers isolés, jamais les caches réels.
    internal CleanupService(IReadOnlyList<CleanupCategory> categories, Action<string> log)
    {
        _categories = categories;
        _log = log;
    }

    public Task<IReadOnlyList<ScanResult>> ScanAsync(IProgress<CleanupProgress> progress,
        CancellationToken token) => Task.Run<IReadOnlyList<ScanResult>>(() =>
    {
        var results = new List<ScanResult>();
        foreach (var category in Categories)
        {
            token.ThrowIfCancellationRequested();
            progress.Report(new($"Analyse : {category.Name}", results.Count, Categories.Count));
            var files = new List<FileCandidate>();
            int skipped = 0;
            bool exists = Directory.Exists(category.Root);
            var pending = new Stack<string>();
            if (exists) pending.Push(category.Root);
            while (pending.TryPop(out string? directory))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (!IsSafePath(category.Root, directory, allowRoot: true)) { skipped++; continue; }
                    foreach (string path in Directory.EnumerateFiles(directory))
                    {
                        token.ThrowIfCancellationRequested();
                        try
                        {
                            if (!IsSafePath(category.Root, path)) { skipped++; continue; }
                            var file = new FileInfo(path);
                            if (IsEligible(category, file))
                                files.Add(new(file.FullName, file.Length, file.LastWriteTimeUtc));
                        }
                        catch (Exception ex) when (IsFileError(ex)) { skipped++; _log(ex.Message); }
                    }
                    foreach (string child in Directory.EnumerateDirectories(directory)) pending.Push(child);
                }
                catch (Exception ex) when (IsFileError(ex)) { skipped++; _log(ex.Message); }
            }
            results.Add(new(category, files.AsReadOnly(), skipped, exists));
        }
        progress.Report(new("Analyse terminée", Categories.Count, Categories.Count));
        return results.AsReadOnly();
    }, token);

    public Task<CleanupResult> CleanAsync(IReadOnlyList<ScanResult> selection,
        IProgress<CleanupProgress> progress, CancellationToken token) => Task.Run(() =>
    {
        int deleted = 0, skipped = 0, completed = 0;
        long bytes = 0;
        int total = selection.Sum(result => result.Files.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (selection.Any(result => !_categories.Contains(result.Category)))
            throw new InvalidOperationException("Catégorie de nettoyage non autorisée.");
        foreach (var result in selection)
        {
            foreach (var candidate in result.Files)
            {
                if (token.IsCancellationRequested) return new CleanupResult(deleted, skipped, bytes, true);
                try
                {
                    var file = new FileInfo(candidate.Path);
                    if (!seen.Add(candidate.Path) || !IsSafePath(result.Category.Root, candidate.Path)
                        || !file.Exists || !IsEligible(result.Category, file)
                        || file.Length != candidate.SizeBytes || file.LastWriteTimeUtc != candidate.LastWriteUtc)
                    {
                        skipped++;
                    }
                    else
                    {
                        // Aucun dossier supprimé, aucun attribut de protection modifié.
                        file.Delete();
                        deleted++;
                        bytes += candidate.SizeBytes;
                    }
                }
                catch (Exception ex) when (IsFileError(ex)) { skipped++; _log(ex.Message); }
                completed++;
                if (completed % 25 == 0 || completed == total)
                    progress.Report(new($"Nettoyage : {completed} / {total} fichiers", completed, total));
            }
        }
        return new CleanupResult(deleted, skipped, bytes, token.IsCancellationRequested);
    });

    private static bool IsEligible(CleanupCategory category, FileInfo file) =>
        (file.Attributes & (FileAttributes.ReparsePoint | FileAttributes.ReadOnly | FileAttributes.System)) == 0
        && file.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-7)
        && (category.Kind != CleanupKind.CrashDumps || file.Extension.Equals(".dmp", StringComparison.OrdinalIgnoreCase));

    internal static bool IsSafePath(string root, string path, bool allowRoot = false)
    {
        string boundary = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        string full = Path.GetFullPath(path);
        if (!(allowRoot && full.Equals(boundary, StringComparison.OrdinalIgnoreCase))
            && !full.StartsWith(boundary + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return false;
        // Vérifie aussi les ancêtres de la racine (profil redirigé, jonction, etc.).
        for (string? current = full; current != null; current = Path.GetDirectoryName(current))
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return false;
        }
        return true;
    }

    private static bool IsFileError(Exception ex) => ex is IOException or UnauthorizedAccessException or SecurityException;
}
