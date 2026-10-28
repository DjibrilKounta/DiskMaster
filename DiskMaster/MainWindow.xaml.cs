using System.ComponentModel;
using System.Windows;
using DiskMaster.ViewModels;

namespace DiskMaster;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    // Comportement propre à la fenêtre : attendre la fin d'une opération.
    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel { IsBusy: true })
        {
            e.Cancel = true;
            MessageBox.Show(this, "Annulez l’opération puis attendez sa fin avant de fermer DiskMaster.",
                "Opération en cours", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        base.OnClosing(e);
    }
}
