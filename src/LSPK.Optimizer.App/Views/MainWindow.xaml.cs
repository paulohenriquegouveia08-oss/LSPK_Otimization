using System.Windows;
using LSPK.Optimizer.App.ViewModels;

namespace LSPK.Optimizer.App.Views;

/// <summary>
/// Código de suporte (code-behind) da Janela Principal do LSPK Optimizer.
/// Trata os eventos dos botões de controle da janela (minimizar, maximizar, fechar)
/// e inscreve-se no evento de conclusão da otimização para abrir o modal de resultados.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// Disparado após a renderização inicial da janela.
    /// Conecta a abertura do modal de relatório ao evento do ViewModel e inicia a leitura de dados.
    /// </summary>
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Assina o evento para abrir o modal com os resultados da otimização
            vm.OnOptimizationFinished += result =>
            {
                Dispatcher.Invoke(() =>
                {
                    var dialog = new ReportDialog(result) { Owner = this };
                    dialog.ShowDialog();
                });
            };

            // Inicializa a telemetria do sistema
            await vm.InitializeAsync();
        }
    }

    /// <summary>Minimiza a janela para a barra de tarefas.</summary>
    private void Minimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>Alterna entre estado Maximizado e Normal da janela.</summary>
    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>Fecha a aplicação com segurança descarregando os recursos.</summary>
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
