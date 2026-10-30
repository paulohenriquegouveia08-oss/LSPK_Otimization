using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;

namespace LSPK.Optimizer.App.ViewModels;

/// <summary>
/// Identificadores das abas principais da barra de navegação lateral.
/// </summary>
public enum NavigationTab
{
    Dashboard,
    Cleaner,
    Startup,
    History
}

/// <summary>
/// ViewModel principal da aplicação.
/// Coordena a navegação entre telas, a telemetria do dashboard, a execução do "OTIMIZAR AGORA",
/// a notificação de atualizações automáticas e o disparo do modal de resultados.
/// </summary>
public class MainViewModel : ViewModelBase
{
    // Serviços injetados
    private readonly IOptimizationOrchestrator _orchestrator;
    private readonly IHardwareInfoService _hardwareService;
    private readonly IHealthScoreCalculator _healthCalculator;
    private readonly IJunkCleanerService _cleanerService;
    private readonly IStartupManagerService _startupService;
    private readonly ILocalHistoryService _historyService;
    private readonly IUpdateService _updateService;

    // Estado da navegação e dados
    private NavigationTab _currentTab = NavigationTab.Dashboard;
    private SystemSpecs _specs = new();
    private SystemHealthReport _health = new() { Score = 85 };

    // Estado da otimização central
    private bool _isOptimizing;
    private int _optStep;
    private string _optTitle = string.Empty;
    private string _optDetail = string.Empty;
    private double _optPercent;
    private CancellationTokenSource? _optCts;

    // Estado da atualização automática
    private bool _isUpdateAvailable;
    private string _updateVersion = string.Empty;
    private string _updateNotes = string.Empty;
    private UpdateManifest? _pendingManifest;
    private string? _downloadedPackagePath;

    // ViewModels filhos gerenciados
    public CleanerViewModel CleanerVM { get; }
    public StartupViewModel StartupVM { get; }
    public HistoryViewModel HistoryVM { get; }

    /// <summary>Aba de navegação atualmente ativa.</summary>
    public NavigationTab CurrentTab
    {
        get => _currentTab;
        set
        {
            if (SetProperty(ref _currentTab, value))
            {
                OnPropertyChanged(nameof(IsDashboardActive));
                OnPropertyChanged(nameof(IsCleanerActive));
                OnPropertyChanged(nameof(IsStartupActive));
                OnPropertyChanged(nameof(IsHistoryActive));
            }
        }
    }

    public bool IsDashboardActive => CurrentTab == NavigationTab.Dashboard;
    public bool IsCleanerActive => CurrentTab == NavigationTab.Cleaner;
    public bool IsStartupActive => CurrentTab == NavigationTab.Startup;
    public bool IsHistoryActive => CurrentTab == NavigationTab.History;

    /// <summary>Especificações físicas de hardware e SO exibidas no Dashboard.</summary>
    public SystemSpecs Specs
    {
        get => _specs;
        set => SetProperty(ref _specs, value);
    }

    /// <summary>Relatório e pontuação de saúde atual do computador.</summary>
    public SystemHealthReport Health
    {
        get => _health;
        set => SetProperty(ref _health, value);
    }

    /// <summary>Indica se a otimização de 1 clique está em andamento (ativa overlay de progresso).</summary>
    public bool IsOptimizing
    {
        get => _isOptimizing;
        set
        {
            if (SetProperty(ref _isOptimizing, value))
            {
                OnPropertyChanged(nameof(CanStartOptimization));
            }
        }
    }

    public bool CanStartOptimization => !IsOptimizing;

    public int OptimizationStep
    {
        get => _optStep;
        set => SetProperty(ref _optStep, value);
    }

    public string OptimizationTitle
    {
        get => _optTitle;
        set => SetProperty(ref _optTitle, value);
    }

    public string OptimizationDetail
    {
        get => _optDetail;
        set => SetProperty(ref _optDetail, value);
    }

    public double OptimizationPercent
    {
        get => _optPercent;
        set => SetProperty(ref _optPercent, value);
    }

    /// <summary>Indica se há nova versão baixada pronta para instalação.</summary>
    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetProperty(ref _isUpdateAvailable, value);
    }

    public string UpdateVersion
    {
        get => _updateVersion;
        set => SetProperty(ref _updateVersion, value);
    }

    // Comandos de Navegação
    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateCleanerCommand { get; }
    public ICommand NavigateStartupCommand { get; }
    public ICommand NavigateHistoryCommand { get; }

    // Comandos de Ação
    public ICommand OptimizeNowCommand { get; }
    public ICommand CancelOptimizationCommand { get; }
    public ICommand ApplyUpdateCommand { get; }

    /// <summary>
    /// Evento disparado para a MainWindow abrir o modal com o relatório final após o término da otimização.
    /// </summary>
    public event Action<OptimizationRunResult>? OnOptimizationFinished;

    public MainViewModel(
        IOptimizationOrchestrator orchestrator,
        IHardwareInfoService hardwareService,
        IHealthScoreCalculator healthCalculator,
        IJunkCleanerService cleanerService,
        IStartupManagerService startupService,
        ILocalHistoryService historyService,
        IUpdateService updateService)
    {
        _orchestrator = orchestrator;
        _hardwareService = hardwareService;
        _healthCalculator = healthCalculator;
        _cleanerService = cleanerService;
        _startupService = startupService;
        _historyService = historyService;
        _updateService = updateService;

        CleanerVM = new CleanerViewModel(cleanerService);
        StartupVM = new StartupViewModel(startupService);
        HistoryVM = new HistoryViewModel(historyService);

        // Inicialização dos comandos de navegação
        NavigateDashboardCommand = new RelayCommand(() => CurrentTab = NavigationTab.Dashboard);
        NavigateCleanerCommand = new RelayCommand(async () =>
        {
            CurrentTab = NavigationTab.Cleaner;
            if (!CleanerVM.Categories.Any()) await CleanerVM.ExecuteScanAsync();
        });
        NavigateStartupCommand = new RelayCommand(async () =>
        {
            CurrentTab = NavigationTab.Startup;
            if (!StartupVM.AllItems.Any()) await StartupVM.LoadEntriesAsync();
        });
        NavigateHistoryCommand = new RelayCommand(async () =>
        {
            CurrentTab = NavigationTab.History;
            await HistoryVM.LoadHistoryAsync();
        });

        // Comandos de execução
        OptimizeNowCommand = new AsyncRelayCommand(ExecuteOptimizeNowAsync, () => CanStartOptimization);
        CancelOptimizationCommand = new RelayCommand(() => _optCts?.Cancel());
        ApplyUpdateCommand = new AsyncRelayCommand(ExecuteApplyUpdateAsync);
    }

    /// <summary>
    /// Inicialização assíncrona chamada quando a MainWindow termina de carregar.
    /// </summary>
    public async Task InitializeAsync()
    {
        // 1. Carrega dados reais de hardware
        Specs = await _hardwareService.GetSystemSpecsAsync();

        // 2. Realiza diagnóstico inicial para compor o score de saúde
        await RefreshHealthScoreAsync();

        // 3. Verifica silenciosamente por atualizações no servidor em background
        _ = CheckUpdatesInBackgroundAsync();
    }

    /// <summary>
    /// Atualiza o score de saúde realizando varredura e checagem de inicializadores.
    /// </summary>
    public async Task RefreshHealthScoreAsync()
    {
        try
        {
            var scans = await _cleanerService.ScanAsync();
            var startups = await _startupService.GetStartupEntriesAsync();
            var history = await _historyService.GetHistoryAsync();

            long junkSize = scans.Where(s => s.IsSelected).Sum(s => s.TotalSizeBytes);
            DateTime? lastOpt = history.FirstOrDefault()?.Timestamp;

            Health = _healthCalculator.Calculate(junkSize, startups, lastOpt);
        }
        catch
        {
            Health = new SystemHealthReport { Score = 85 };
        }
    }

    /// <summary>
    /// Executa o pipeline de 9 etapas do botão "OTIMIZAR AGORA" com telemetria em tempo real.
    /// </summary>
    public async Task ExecuteOptimizeNowAsync()
    {
        if (IsOptimizing) return;

        IsOptimizing = true;
        _optCts = new CancellationTokenSource();
        OptimizationPercent = 0;
        OptimizationStep = 1;
        OptimizationTitle = "Iniciando otimização do sistema...";
        OptimizationDetail = "Preparando módulos de limpeza e inicialização";

        // Callback para atualização da interface
        var progress = new Progress<OptimizationProgress>(p =>
        {
            OptimizationStep = p.CurrentStep;
            OptimizationTitle = p.StepTitle;
            OptimizationDetail = p.CurrentDetail;
            OptimizationPercent = p.OverallPercent;
        });

        try
        {
            var result = await _orchestrator.ExecuteFullOptimizationAsync(_optCts.Token, progress);

            // Recalcula o score após a liberação física de espaço
            await RefreshHealthScoreAsync();

            // Abre o modal de relatório final para o usuário
            OnOptimizationFinished?.Invoke(result);
        }
        finally
        {
            IsOptimizing = false;
            _optCts?.Dispose();
            _optCts = null;
        }
    }

    /// <summary>
    /// Consulta o manifesto updates/latest.json e faz download silencioso do pacote se for superior.
    /// </summary>
    private async Task CheckUpdatesInBackgroundAsync()
    {
        try
        {
            const string manifestUrl = "https://updates.lspktech.com/optimizer/updates/latest.json";
            var check = await _updateService.CheckForUpdatesAsync("1.0.0", manifestUrl);

            if (check.UpdateAvailable && check.Manifest != null)
            {
                _pendingManifest = check.Manifest;
                UpdateVersion = check.Manifest.Version;
                IsUpdateAvailable = true;

                // Baixa o pacote em segundo plano sem travar a UI
                _downloadedPackagePath = await _updateService.DownloadUpdatePackageAsync(check.Manifest);
            }
        }
        catch
        {
            // Silencioso em caso de ausência de conexão com a internet
        }
    }

    /// <summary>
    /// Inicia o atualizador independente LSPK.Optimizer.Updater.exe e fecha o aplicativo principal.
    /// </summary>
    private async Task ExecuteApplyUpdateAsync()
    {
        if (IsOptimizing) return; // Aguarda otimização em andamento concluir

        if (string.IsNullOrEmpty(_downloadedPackagePath) && _pendingManifest != null)
        {
            _downloadedPackagePath = await _updateService.DownloadUpdatePackageAsync(_pendingManifest);
        }

        if (!string.IsNullOrEmpty(_downloadedPackagePath) && File.Exists(_downloadedPackagePath))
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string updaterExe = Path.Combine(currentDir, "LSPK.Optimizer.Updater.exe");

            if (File.Exists(updaterExe))
            {
                // Dispara o atualizador desacoplado passando o PID atual e o caminho do pacote
                Process.Start(new ProcessStartInfo
                {
                    FileName = updaterExe,
                    Arguments = $"--pid {Environment.ProcessId} --package \"{_downloadedPackagePath}\" --target \"{currentDir}\"",
                    UseShellExecute = true
                });

                // Encerra a aplicação para liberar os arquivos para substituição
                Environment.Exit(0);
            }
        }
    }
}
