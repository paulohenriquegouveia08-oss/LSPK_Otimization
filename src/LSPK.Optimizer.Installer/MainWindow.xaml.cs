using System.Diagnostics;
using System.IO;
using System.Windows;
using LSPK.Optimizer.Installer.Services;

namespace LSPK.Optimizer.Installer;

/// <summary>
/// Código de suporte da interface gráfica do instalador personalizado LSPKOptimizerSetup.
/// Controla o fluxo de telas (Boas-vindas, Instalação em Andamento, Concluída)
/// e vincula a barra de progresso aos bytes reais processados.
/// </summary>
public partial class MainWindow : Window
{
    private readonly InstallationService _installService = new();
    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
        TxtInstallPath.Text = InstallationService.DefaultInstallPath;
    }

    /// <summary>
    /// Evento disparado ao clicar em "Instalar Agora".
    /// Oculta a tela de introdução, exibe a barra de progresso e inicia a extração assíncrona.
    /// </summary>
    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        StepWelcome.Visibility = Visibility.Collapsed;
        StepInstalling.Visibility = Visibility.Visible;
        BtnInstall.Visibility = Visibility.Collapsed;
        BtnCancel.IsEnabled = true;

        _cts = new CancellationTokenSource();

        // Callback de progresso real atualizando os textos e a barra da janela
        var progress = new Progress<InstallProgress>(p =>
        {
            Dispatcher.Invoke(() =>
            {
                TxtStepDetail.Text = p.StatusDescription;
                PrgInstall.Value = p.Percent;
                TxtFilesProgress.Text = $"{p.FilesExtracted} / {p.TotalFiles} arquivos processados";
                TxtBytesProgress.Text = $"{JunkCategoryScanResult.FormatBytes(p.BytesWritten)} / {JunkCategoryScanResult.FormatBytes(p.TotalBytes)}";
            });
        });

        bool desktop = ChkDesktopShortcut.IsChecked ?? true;
        string targetDir = InstallationService.DefaultInstallPath;

        // Executa a instalação física
        bool success = await _installService.InstallAsync(targetDir, desktop, progress, _cts.Token);

        if (success)
        {
            // Avança para a tela de conclusão
            StepInstalling.Visibility = Visibility.Collapsed;
            StepCompleted.Visibility = Visibility.Visible;
            BtnCancel.Visibility = Visibility.Collapsed;
            BtnFinish.Visibility = Visibility.Visible;
        }
        else
        {
            MessageBox.Show("Ocorreu uma falha durante a instalação.", "Erro no Instalador", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Finaliza o assistente de instalação e, se marcado, inicia o LSPK.Optimizer.App.exe.
    /// </summary>
    private void Finish_Click(object sender, RoutedEventArgs e)
    {
        if (ChkLaunchApp.IsChecked == true)
        {
            string appExe = Path.Combine(InstallationService.DefaultInstallPath, "LSPK.Optimizer.App.exe");
            if (File.Exists(appExe))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = appExe,
                    UseShellExecute = true
                });
            }
        }
        Close();
    }

    /// <summary>Cancela a instalação em andamento ou fecha a janela.</summary>
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        Close();
    }
}
