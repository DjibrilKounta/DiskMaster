using System.Diagnostics;
using System.Windows;

namespace DiskMaster.Services;

public interface IDesktopService
{
    bool Confirm(string message);
    void ShowError(string message);
    void OpenFolder(string path);
    void OpenStorageSettings();
}

public sealed class DesktopService : IDesktopService
{
    public bool Confirm(string message) => MessageBox.Show(Application.Current.MainWindow,
        message, "Confirmer le nettoyage", MessageBoxButton.YesNo,
        MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes;

    public void ShowError(string message) => MessageBox.Show(Application.Current.MainWindow,
        message, "DiskMaster", MessageBoxButton.OK, MessageBoxImage.Error);

    public void OpenFolder(string path) => Process.Start(new ProcessStartInfo("explorer.exe")
    {
        UseShellExecute = true,
        ArgumentList = { path }
    });

    public void OpenStorageSettings() => Process.Start(new ProcessStartInfo("ms-settings:storagesense")
    { UseShellExecute = true });
}
