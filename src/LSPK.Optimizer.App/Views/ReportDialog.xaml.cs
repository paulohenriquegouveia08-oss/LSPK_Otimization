using System.Windows;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;

namespace LSPK.Optimizer.App.Views;

/// <summary>
/// Caixa de diálogo modal moderna exibida ao usuário imediatamente após a conclusão
/// da otimização de 1 clique, apresentando métricas físicas consolidadas.
/// </summary>
public partial class ReportDialog : Window
{
    /// <summary>
    /// Construtor que recebe os resultados auditáveis da otimização e popula os elementos visuais.
    /// </summary>
    /// <param name="result">Objeto contendo bytes liberados, arquivos apagados e scores de saúde.</param>
    public partial class Dummy { }

    public ReportDialog(OptimizationRunResult result)
    {
        InitializeComponent();

        // Formata os bytes físicos recuperados (ex: "3.2 GB")
        TxtBytesFreed.Text = JunkCategoryScanResult.FormatBytes(result.BytesFreed);

        // Exibe quantidade de arquivos deletados
        TxtFilesDeleted.Text = $"{result.FilesDeleted} arquivos excluídos";

        // Exibe a nova pontuação de saúde alcançada
        TxtHealthScore.Text = $"{result.FinalHealthScore} / 100";

        // Exibe a quantidade de itens órfãos desativados
        TxtStartupOptimized.Text = $"{result.StartupItemsOptimized} inicializadores ajustados";
    }

    /// <summary>Fecha a janela modal retornando o foco à MainWindow.</summary>
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
