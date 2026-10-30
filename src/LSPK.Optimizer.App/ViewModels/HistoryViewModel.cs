using System.Collections.ObjectModel;
using System.Windows.Input;
using LSPK.Optimizer.Core.Models;
using LSPK.Optimizer.Core.Services;

namespace LSPK.Optimizer.App.ViewModels;

/// <summary>
/// ViewModel responsável pela tela de Histórico de Otimizações.
/// Gerencia a listagem cronológica das manutenções passadas e a soma acumulada de gigabytes liberados.
/// </summary>
public class HistoryViewModel : ViewModelBase
{
    private readonly ILocalHistoryService _historyService;
    private bool _isLoading;
    private long _totalHistoricalFreedBytes;

    /// <summary>Coleção observável de registros históricos exibida na interface.</summary>
    public ObservableCollection<OptimizationHistoryItem> HistoryItems { get; } = new();

    /// <summary>Indica se os registros estão sendo lidos do arquivo JSON no momento.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>Texto formatado com o total histórico acumulado de espaço físico liberado no PC.</summary>
    public string TotalHistoricalFreed => JunkCategoryScanResult.FormatBytes(_totalHistoricalFreedBytes);

    /// <summary>Comando para recarregar o histórico a partir do disco.</summary>
    public ICommand RefreshCommand { get; }

    public HistoryViewModel(ILocalHistoryService historyService)
    {
        _historyService = historyService;
        RefreshCommand = new AsyncRelayCommand(LoadHistoryAsync);
    }

    /// <summary>
    /// Lê assincronamente os registros gravados no histórico local e atualiza a soma acumulada.
    /// </summary>
    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        try
        {
            var records = await _historyService.GetHistoryAsync();
            HistoryItems.Clear();

            _totalHistoricalFreedBytes = 0;
            foreach (var r in records)
            {
                HistoryItems.Add(r);
                _totalHistoricalFreedBytes += r.BytesFreed;
            }

            OnPropertyChanged(nameof(TotalHistoricalFreed));
        }
        finally
        {
            IsLoading = false;
        }
    }
}
