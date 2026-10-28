using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using DiskMaster.Services;
using DiskMaster.ViewModels;

namespace DiskMaster;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        // Composition root : les dépendances sont assemblées ici, une seule fois.
        var data = new UserDataService();
        var cleanup = new CleanupService(data.Log);
        var desktop = new DesktopService();
        var viewModel = new MainViewModel(cleanup, data, desktop);
        MainWindow = new MainWindow(viewModel);
        MainWindow.Show();
    }
}
