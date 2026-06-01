// Classe auxiliar ajustada
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Func<object, Task> _executeAsyncWithParam;
    private readonly Func<Task> _executeAsyncNoParam;
    private readonly Action<object> _executeWithParam;
    private readonly Action _executeNoParam;

    // Adicionado: Guardará a regra de validação do botão
    private readonly Predicate<object> _canExecute;

    // Construtores atualizados aceitando o canExecute opcional
    public RelayCommand(Func<Task> executeAsync, Predicate<object> canExecute = null)
    {
        _executeAsyncNoParam = executeAsync;
        _canExecute = canExecute;
    }

    public RelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute = null)
    {
        _executeAsyncWithParam = executeAsync;
        _canExecute = canExecute;
    }

    public RelayCommand(Action execute, Predicate<object> canExecute = null)
    {
        _executeNoParam = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _executeWithParam = execute;
        _canExecute = canExecute;
    }

    // Vincula o evento para o WPF atualizar o estado do botão na tela automaticamente
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    // Avalia se o botão deve estar ativo ou não
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public async void Execute(object parameter)
    {
        if (_executeAsyncWithParam != null) await _executeAsyncWithParam(parameter);
        else if (_executeAsyncNoParam != null) await _executeAsyncNoParam();
        else if (_executeWithParam != null) _executeWithParam(parameter);
        else _executeNoParam?.Invoke();
    }
}
