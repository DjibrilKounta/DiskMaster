namespace DiskMaster.Models;

public enum CleanupKind { TemporaryFiles, DirectXCache, CrashDumps }

public sealed record CleanupCategory(CleanupKind Kind, string Name, string Description, string Root);

// Un instantané : le nettoyage ne découvre jamais de nouveaux fichiers à supprimer.
public sealed record FileCandidate(string Path, long SizeBytes, DateTime LastWriteUtc);
public sealed record ScanResult(CleanupCategory Category, IReadOnlyList<FileCandidate> Files,
    int Skipped, bool Exists)
{
    public long SizeBytes => Files.Sum(file => file.SizeBytes);
}

public sealed record CleanupProgress(string Message, int Completed, int Total);
public sealed record CleanupResult(int Deleted, int Skipped, long DeletedBytes, bool Canceled);
public sealed record CleanupHistory(DateTimeOffset Date, int Deleted, int Skipped,
    long DeletedBytes, bool Canceled);
