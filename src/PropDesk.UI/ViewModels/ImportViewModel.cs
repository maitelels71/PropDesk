using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PropDesk.Domain.Models;
using PropDesk.Infrastructure.Import;
using System.Collections.ObjectModel;

namespace PropDesk.UI.ViewModels;

public partial class ImportViewModel : ObservableObject
{
    private readonly int _accountId;
    public Action? CloseAction { get; set; }
    public Action<List<Trade>>? OnImportComplete { get; set; }

    [ObservableProperty] string  selectedFilePath = "No file selected...";
    [ObservableProperty] int     totalTrades;
    [ObservableProperty] int     tradingDaysCount;
    [ObservableProperty] decimal importNetPnL;
    [ObservableProperty] int     warningCount;
    [ObservableProperty] bool    canImport;

    public ObservableCollection<Trade> PreviewTrades { get; } = new();

    private List<Trade> _allTrades = new();

    public ImportViewModel(int accountId = 1)
    {
        _accountId = accountId;
    }

    [RelayCommand]
    private void Browse()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Select MyFundedFutures CSV Export",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true) return;

        SelectedFilePath = dlg.FileName;
        LoadPreview(dlg.FileName);
    }

    private void LoadPreview(string path)
    {
        var result = CsvImporter.ImportFromFile(path, _accountId);

        _allTrades = result.Trades;
        TotalTrades      = result.TotalImported;
        WarningCount     = result.Warnings.Count;
        TradingDaysCount = result.Trades.Select(t => t.Date.Date).Distinct().Count();
        ImportNetPnL     = result.Trades.Sum(t => t.NetProfit);
        CanImport        = result.Success && result.TotalImported > 0;

        PreviewTrades.Clear();
        foreach (var t in result.Trades.Take(20))
            PreviewTrades.Add(t);
    }

    [RelayCommand]
    private void Import()
    {
        if (_allTrades.Count == 0) return;
        OnImportComplete?.Invoke(_allTrades);
        CloseAction?.Invoke();
    }
}