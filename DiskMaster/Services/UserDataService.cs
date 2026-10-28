using System.IO;
using System.Text.Json;
using DiskMaster.Models;

namespace DiskMaster.Services;

public interface IUserDataService
{
    CleanupHistory? Load();
    void Save(CleanupHistory history);
    void Log(string message);
    string Folder { get; }
}

public sealed class UserDataService : IUserDataService
{
    public string Folder { get; }
    public UserDataService() : this(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DiskMaster")) { }

    internal UserDataService(string folder) => Folder = Path.GetFullPath(folder);
    private string HistoryPath => Path.Combine(Folder, "history.json");

    public CleanupHistory? Load()
    {
        try
        {
            return File.Exists(HistoryPath)
                ? JsonSerializer.Deserialize<CleanupHistory>(File.ReadAllText(HistoryPath)) : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            Log($"Historique indisponible : {ex.Message}");
            return null;
        }
    }

    public void Save(CleanupHistory history)
    {
        Directory.CreateDirectory(Folder);
        string temporary = Path.Combine(Folder, $"history-{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(history));
            File.Move(temporary, HistoryPath, overwrite: true);
        }
        finally
        {
            try { File.Delete(temporary); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { Log($"Fichier temporaire conservé : {ex.Message}"); }
        }
    }

    public void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            string path = Path.Combine(Folder, "diagnostic.log");
            if (File.Exists(path) && new FileInfo(path).Length > 1_000_000)
                File.Move(path, path + ".old", overwrite: true);
            File.AppendAllText(path, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { System.Diagnostics.Debug.WriteLine(ex); }
    }
}
