using System.Windows.Input;

namespace DiskMaster.Infrastructure;

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null, Action<Exception>? onError = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        try { execute(); }
        catch (Exception ex) when (onError is not null) { onError(ex); }
    }
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public sealed class AsyncCommand(Func<Task> execute, Action<Exception> onError,
    Func<bool>? canExecute = null) : ICommand
{
    private bool _running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !_running && (canExecute?.Invoke() ?? true);
    public async void Execute(object? parameter) => await ExecuteAsync();

    /// <summary>Exécute la commande et permet aux tests d'attendre sa fin.</summary>
    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;
        _running = true;
        Refresh();
        try { await execute(); }
        catch (Exception ex) { onError(ex); }
        finally { _running = false; Refresh(); }
    }
    public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
