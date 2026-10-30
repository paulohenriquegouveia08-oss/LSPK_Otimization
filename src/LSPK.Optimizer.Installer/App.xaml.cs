using System.Windows;
using LSPK.Optimizer.Installer.Services;

namespace LSPK.Optimizer.Installer;

/// <summary>
/// Ponto de entrada do instalador standalone LSPKOptimizerSetup.exe.
/// Avalia se foi chamado com argumentos de desinstalação (--uninstall) ou se deve abrir a interface do instalador.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Evento disparado na inicialização do instalador.
    /// </summary>
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Se o executável foi chamado com argumento --uninstall (acionado pelo Painel de Controle)
        if (e.Args.Length > 0 && e.Args[0].Equals("--uninstall", StringComparison.OrdinalIgnoreCase))
        {
            var result = MessageBox.Show(
                "Tem certeza que deseja desinstalar o LSPK Optimizer deste computador?",
                "Desinstalação do LSPK Optimizer",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                await InstallationService.UninstallAsync();
                MessageBox.Show("O LSPK Optimizer foi desinstalado com sucesso.", "Desinstalação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Shutdown();
            return;
        }

        // Execução convencional: abre a janela personalizada do assistente de instalação
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
