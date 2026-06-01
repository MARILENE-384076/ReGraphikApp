// Classe auxiliar ajustada
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Func<Task>? _executeAsync;
    private readonly Action? _execute;

    // Construtor para comandos assíncronos (recomendo usar este para novos comandos)
    public RelayCommand(Func<Task> executeAsync) => _executeAsync = executeAsync;

    // Construtor para comandos síncronos (mantido para compatibilidade, mas prefira o assíncrono)
    public RelayCommand(Func<Task> executeAsync, Func<object, bool> value) => _executeAsync = executeAsync;
    public RelayCommand(Action execute) => _execute = execute;

    // O evento CanExecuteChanged é necessário para que a interface do WPF saiba quando reavaliar se o comando pode ser executado
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Para simplificar, este comando sempre pode ser executado. Se precisar de lógica para habilitar/desabilitar o comando, ajuste este método.
    public bool CanExecute(object? parameter) => true;

    public async void Execute(object? parameter)
    {
        if (_executeAsync != null) await _executeAsync();
        else _execute?.Invoke();
    }
}