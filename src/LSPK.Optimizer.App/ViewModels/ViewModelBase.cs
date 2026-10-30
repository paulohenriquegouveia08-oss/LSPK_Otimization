using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LSPK.Optimizer.App.ViewModels;

/// <summary>
/// Classe base para ViewModels implementando o padrão MVVM (Model-View-ViewModel)
/// com notificação automática de alteração de propriedades para atualização da interface WPF.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>Evento padrão do WPF disparado quando o valor de uma propriedade vinculada é alterado.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Método auxiliar para atribuir valor a um campo de suporte e disparar o evento PropertyChanged
    /// apenas se o novo valor for realmente diferente do valor atual.
    /// </summary>
    /// <typeparam name="T">Tipo do dado da propriedade.</typeparam>
    /// <param name="field">Referência ao campo privado de suporte.</param>
    /// <param name="value">Novo valor a ser atribuído.</param>
    /// <param name="propertyName">Nome da propriedade chamadora (preenchido automaticamente pelo compilador).</param>
    /// <returns>Verdadeiro se o valor mudou; Falso se era igual.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Dispara a notificação de mudança para a interface gráfica redesenhar o elemento vinculado.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Implementação síncrona do padrão de comando ICommand para vinculação com botões do XAML.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
    {
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => _execute(parameter);
}

/// <summary>
/// Implementação assíncrona de ICommand para execução de operações pesadas sem congelar a thread de interface.
/// Desativa automaticamente o botão enquanto a tarefa assíncrona estiver em andamento.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        _isExecuting = true;
        CommandManager.InvalidateRequerySuggested();
        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
