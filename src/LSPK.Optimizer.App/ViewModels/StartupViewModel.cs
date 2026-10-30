using System.Collections.ObjectModel;
using System.Windows.Input;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;

namespace LSPK.Optimizer.App.ViewModels;

/// <summary>
/// Wrapper de ViewModel para um item individual de inicialização,
/// gerenciando a alternância do switch (Ativado/Desativado) e seu estado ocupado.
/// </summary>
public class StartupItemViewModel : ViewModelBase
{
    private readonly StartupEntry _model;
    private readonly IStartupManagerService _service;
    private bool _isEnabled;
    private bool _isBusy;

    public StartupItemViewModel(StartupEntry model, IStartupManagerService service)
    {
        _model = model;
        _service = service;
        _isEnabled = model.IsEnabled;
    }

    /// <summary>Modelo de dados puro subjacente.</summary>
    public StartupEntry Model => _model;

    /// <summary>Nome do programa ou chave.</summary>
    public string Name => _model.Name;

    /// <summary>Fabricante do software.</summary>
    public string Publisher => _model.Publisher;

    /// <summary>Caminho do executável.</summary>
    public string ExecutablePath => _model.ExecutablePath;

    /// <summary>Descrição em português da origem física.</summary>
    public string SourceDescription => _model.SourceDescription;

    /// <summary>Indica se o executável existe no disco.</summary>
    public bool TargetExists => _model.TargetExists;

    /// <summary>Indica se a entrada é órfã (o programa foi desinstalado mas sobrou no registro).</summary>
    public bool IsOrphaned => _model.IsOrphaned;

    /// <summary>Indica se o item é protegido pelo sistema (antivírus, drivers, windows core).</summary>
    public bool IsSystemProtected => _model.IsSystemProtected;

    /// <summary>Nível de impacto estimado no tempo de boot.</summary>
    public StartupImpact Impact => _model.Impact;

    /// <summary>
    /// Estado do switch de ativação. Dispara a desativação segura ou reativação via serviço.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (value != _isEnabled && !IsSystemProtected && !_isBusy)
            {
                _ = ToggleStateAsync(value);
            }
        }
    }

    /// <summary>Define se o switch pode ser clicado pelo usuário (desabilitado para itens do sistema).</summary>
    public bool CanToggle => !IsSystemProtected && !_isBusy;

    /// <summary>
    /// Alterna assincronamente o estado de inicialização no registro ou pasta Startup.
    /// </summary>
    private async Task ToggleStateAsync(bool enable)
    {
        _isBusy = true;
        OnPropertyChanged(nameof(CanToggle));

        bool success;
        if (enable)
        {
            success = await _service.EnableEntryAsync(_model);
        }
        else
        {
            success = await _service.DisableEntryAsync(_model);
        }

        if (success)
        {
            _isEnabled = enable;
            OnPropertyChanged(nameof(IsEnabled));
        }

        _isBusy = false;
        OnPropertyChanged(nameof(CanToggle));
    }
}

/// <summary>
/// ViewModel da tela do Módulo 2 (Gerenciamento de Inicialização).
/// Controla listagem, filtragem de texto por busca, limpeza de órfãos e contagem de itens ativos.
/// </summary>
public class StartupViewModel : ViewModelBase
{
    private readonly IStartupManagerService _startupService;
    private bool _isBusy;
    private string _searchText = string.Empty;
    private string _statusMessage = "Carregando inicializadores...";

    /// <summary>Todos os itens carregados da máquina.</summary>
    public ObservableCollection<StartupItemViewModel> AllItems { get; } = new();

    /// <summary>Itens filtrados exibidos na tabela após busca do usuário.</summary>
    public ObservableCollection<StartupItemViewModel> FilteredItems { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>Termo de pesquisa digitado na caixa de busca para filtrar a tabela.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Total de programas órfãos detectados.</summary>
    public int OrphanCount => AllItems.Count(i => i.IsOrphaned);

    /// <summary>Total de programas ativos que iniciam no boot.</summary>
    public int ActiveCount => AllItems.Count(i => i.IsEnabled);

    public ICommand RefreshCommand { get; }
    public ICommand CleanOrphansCommand { get; }

    public StartupViewModel(IStartupManagerService startupService)
    {
        _startupService = startupService;
        RefreshCommand = new AsyncRelayCommand(LoadEntriesAsync);
        CleanOrphansCommand = new AsyncRelayCommand(CleanOrphansAsync);
    }

    /// <summary>
    /// Audita o registro do Windows e pastas Startup, atualizando as coleções.
    /// </summary>
    public async Task LoadEntriesAsync()
    {
        IsBusy = true;
        StatusMessage = "Auditando registro e pastas de inicialização...";

        try
        {
            var entries = await _startupService.GetStartupEntriesAsync();
            AllItems.Clear();

            foreach (var e in entries)
            {
                AllItems.Add(new StartupItemViewModel(e, _startupService));
            }

            ApplyFilter();
            StatusMessage = $"{AllItems.Count} itens identificados ({ActiveCount} ativos, {OrphanCount} órfãos)";
            OnPropertyChanged(nameof(OrphanCount));
            OnPropertyChanged(nameof(ActiveCount));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro ao carregar inicializadores: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Remove todas as entradas de inicialização órfãs que apontam para executáveis inexistentes.
    /// </summary>
    public async Task CleanOrphansAsync()
    {
        var orphans = AllItems.Where(i => i.IsOrphaned && !i.IsSystemProtected).ToList();
        if (!orphans.Any()) return;

        IsBusy = true;
        StatusMessage = $"Removendo {orphans.Count} resquícios de programas órfãos...";

        try
        {
            foreach (var item in orphans)
            {
                await _startupService.RemoveOrphanedEntryAsync(item.Model);
            }
            await LoadEntriesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Aplica o filtro de texto pelo nome, fabricante ou caminho do executável.
    /// </summary>
    private void ApplyFilter()
    {
        FilteredItems.Clear();
        var query = AllItems.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(i =>
                i.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.Publisher.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                i.ExecutablePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var item in query)
        {
            FilteredItems.Add(item);
        }
    }
}
