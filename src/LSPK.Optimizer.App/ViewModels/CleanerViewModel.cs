using System.Collections.ObjectModel;
using System.Windows.Input;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;

namespace LSPK.Optimizer.App.ViewModels;

/// <summary>
/// ViewModel responsável pelo gerenciamento da tela do Módulo 1 (Limpeza de Arquivos).
/// Controla a seleção de categorias, exibição do progresso em tempo real e acionamento dos serviços de limpeza.
/// </summary>
public class CleanerViewModel : ViewModelBase
{
    private readonly IJunkCleanerService _cleanerService;
    private bool _isBusy;
    private string _statusMessage = "Pronto para diagnóstico";
    private double _progressPercent;
    private string _currentDetail = string.Empty;

    /// <summary>Coleção observável de categorias de limpeza vinculada à lista na interface gráfica.</summary>
    public ObservableCollection<JunkCategoryScanResult> Categories { get; } = new();

    /// <summary>Indica se uma varredura ou limpeza está em execução no momento.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        set { if (SetProperty(ref _isBusy, value)) OnPropertyChanged(nameof(CanOperate)); }
    }

    /// <summary>Define se os botões de ação podem ser clicados pelo usuário.</summary>
    public bool CanOperate => !IsBusy;

    /// <summary>Texto principal de status da operação (ex: "Analisando Arquivos Temporários...").</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Percentual concluído (0 a 100%) da barra de progresso.</summary>
    public double ProgressPercent
    {
        get => _progressPercent;
        set => SetProperty(ref _progressPercent, value);
    }

    /// <summary>Detalhe técnico ou arquivo sendo examinado no momento.</summary>
    public string CurrentDetail
    {
        get => _currentDetail;
        set => SetProperty(ref _currentDetail, value);
    }

    /// <summary>Soma total do espaço estimado selecionado para liberação formatado amigavelmente.</summary>
    public string TotalEstimatedFreed => JunkCategoryScanResult.FormatBytes(
        Categories.Where(c => c.IsSelected).Sum(c => c.TotalSizeBytes)
    );

    // Comandos expostos para a View
    public ICommand ScanCommand { get; }
    public ICommand CleanCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand DeselectAllCommand { get; }

    public CleanerViewModel(IJunkCleanerService cleanerService)
    {
        _cleanerService = cleanerService;

        // Comandos assíncronos vinculados aos métodos de negócio
        ScanCommand = new AsyncRelayCommand(ExecuteScanAsync, () => CanOperate);
        CleanCommand = new AsyncRelayCommand(ExecuteCleanAsync, () => CanOperate && Categories.Any(c => c.IsSelected && c.TotalSizeBytes > 0));
        SelectAllCommand = new RelayCommand(() => SetAllSelection(true));
        DeselectAllCommand = new RelayCommand(() => SetAllSelection(false));
    }

    /// <summary>
    /// Dispara a varredura prévia sem apagar arquivos, populando as categorias com tamanhos reais.
    /// </summary>
    public async Task ExecuteScanAsync()
    {
        IsBusy = true;
        StatusMessage = "Executando varredura profunda...";
        ProgressPercent = 0;

        // Assinatura de eventos de progresso para atualização imediata da tela
        var progress = new Progress<OptimizationProgress>(p =>
        {
            StatusMessage = p.StepTitle;
            CurrentDetail = p.CurrentDetail;
            ProgressPercent = p.OverallPercent;
        });

        try
        {
            var results = await _cleanerService.ScanAsync(default, progress);
            Categories.Clear();
            foreach (var r in results)
            {
                Categories.Add(r);
            }
            StatusMessage = $"Varredura concluída: {TotalEstimatedFreed} identificados";
            CurrentDetail = $"{Categories.Sum(c => c.FileCount)} arquivos analisados";
            OnPropertyChanged(nameof(TotalEstimatedFreed));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro durante a varredura: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ProgressPercent = 100;
        }
    }

    /// <summary>
    /// Executa a limpeza física das categorias selecionadas pelo usuário.
    /// </summary>
    public async Task ExecuteCleanAsync()
    {
        IsBusy = true;
        StatusMessage = "Iniciando limpeza seletiva...";
        ProgressPercent = 0;

        var progress = new Progress<OptimizationProgress>(p =>
        {
            StatusMessage = p.StepTitle;
            CurrentDetail = p.CurrentDetail;
            ProgressPercent = p.OverallPercent;
        });

        try
        {
            var report = await _cleanerService.CleanAsync(Categories, default, progress);
            StatusMessage = $"Limpeza finalizada com êxito!";
            CurrentDetail = $"{report.FormattedTotalFreed} liberados | {report.TotalFilesDeleted} arquivos excluídos";
            
            // Reexecuta uma varredura para atualizar a lista com os novos valores
            await ExecuteScanAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erro durante a limpeza: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Marca ou desmarca todas as categorias, preservando a Lixeira desmarcada por segurança.
    /// </summary>
    private void SetAllSelection(bool isSelected)
    {
        foreach (var c in Categories)
        {
            if (c.RequiresConfirmation && isSelected) continue; // Mantém a Lixeira sob confirmação individual
            c.IsSelected = isSelected;
        }
        OnPropertyChanged(nameof(TotalEstimatedFreed));
    }
}
