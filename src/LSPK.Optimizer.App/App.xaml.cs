using System.Windows;
using LSPK.Optimizer.App.ViewModels;
using LSPK.Optimizer.App.Views;
using LSPK.Optimizer.Core.Services;
using Serilog;

namespace LSPK.Optimizer.App;

/// <summary>
/// Ponto de entrada principal da aplicação desktop WPF do LSPK Optimizer.
/// Configura o pipeline de logging (Serilog), registra as instâncias de serviços
/// e inicializa a interface gráfica com o ViewModel principal.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Evento disparado na inicialização do aplicativo antes da exibição de qualquer janela.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Configuração do Serilog para gravação de logs diários em %LocalAppData%\LSPK Optimizer\Logs
        string logDir = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "LSPK Optimizer",
            "Logs"
        );
        System.IO.Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                System.IO.Path.Combine(logDir, "lspk-optimizer-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14, // Mantém os últimos 14 dias de log
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        Log.Information("LSPK Optimizer inicializado. Versão 1.0.0 (MVP)");

        // 2. Composição e Injeção de Dependências de Serviços (Inversão de Controle)
        var cleanerService = new JunkCleanerService();
        var startupService = new StartupManagerService();
        var hardwareService = new HardwareInfoService();
        var healthCalculator = new HealthScoreCalculator();
        var historyService = new LocalHistoryService();
        var updateService = new UpdateService();

        // Orquestrador que coordena o fluxo de 9 etapas do botão central
        var orchestrator = new OptimizationOrchestrator(
            cleanerService,
            startupService,
            hardwareService,
            healthCalculator,
            historyService
        );

        // Instanciação do ViewModel principal da aplicação
        var mainVm = new MainViewModel(
            orchestrator,
            hardwareService,
            healthCalculator,
            cleanerService,
            startupService,
            historyService,
            updateService
        );

        // 3. Inicialização e exibição da Janela Principal (MainWindow)
        var mainWindow = new MainWindow
        {
            DataContext = mainVm
        };

        mainWindow.Show();
    }

    /// <summary>
    /// Evento disparado quando o aplicativo é encerrado pelo usuário ou pelo sistema.
    /// Garante que todos os buffers de log em disco sejam descarregados com segurança.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("LSPK Optimizer finalizado.");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
