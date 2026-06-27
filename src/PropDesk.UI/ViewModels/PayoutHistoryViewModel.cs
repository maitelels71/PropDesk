using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PropDesk.Domain.Models;
using PropDesk.Infrastructure.Database;
using PropDesk.Reporting;
using System.Collections.ObjectModel;

namespace PropDesk.UI.ViewModels;

public partial class PayoutHistoryViewModel : ObservableObject
{
    private readonly PayoutCycleRepository _repo;
    private readonly int _accountId;

    [ObservableProperty] int     totalCycles;
    [ObservableProperty] decimal totalPaid;
    [ObservableProperty] decimal avgPerCycle;
    [ObservableProperty] decimal bestCycle;

    public ObservableCollection<PayoutCycle> Cycles { get; } = new();

    public PayoutHistoryViewModel(DatabaseContext db, int accountId)
    {
        _repo      = new PayoutCycleRepository(db);
        _accountId = accountId;
        Load();
    }

    private void Load()
    {
        var cycles = _repo.GetByAccount(_accountId);
        Cycles.Clear();
        foreach (var c in cycles) Cycles.Add(c);

        TotalCycles  = cycles.Count;
        TotalPaid    = cycles.Sum(c => c.AmountPaid);
        AvgPerCycle  = TotalCycles > 0 ? TotalPaid / TotalCycles : 0m;
        BestCycle    = cycles.Any() ? cycles.Max(c => c.AmountPaid) : 0m;
    }

    [RelayCommand]
    private void ExportPdf()
    {
        var dlg = new SaveFileDialog
        {
            Title    = "Export Payout History PDF",
            Filter   = "PDF files (*.pdf)|*.pdf",
            FileName = $"PropDesk_PayoutHistory_{DateTime.Now:yyyy-MM-dd}.pdf"
        };
        if (dlg.ShowDialog() != true) return;

        var report = new PdfReportService();
        report.ExportPayoutHistory(dlg.FileName, Cycles.ToList(), TotalPaid);
    }
}