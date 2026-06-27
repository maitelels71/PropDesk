using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropDesk.Core.Engines;
using System.Collections.ObjectModel;

namespace PropDesk.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── Account Settings ────────────────────────────────────────
    [ObservableProperty] decimal startingBalance = 50_000m;
    [ObservableProperty] decimal profitTarget    = 3_000m;
    [ObservableProperty] decimal maxDrawdown     = 2_500m;
    [ObservableProperty] decimal dailyDrawdown   = 1_500m;
    [ObservableProperty] decimal requiredBuffer  = 2_500m;
    [ObservableProperty] int     minTradingDays  = 8;

    // ── Live Data (from trades/calendar) ────────────────────────
    [ObservableProperty] decimal currentBalance;
    [ObservableProperty] decimal netProfit;
    [ObservableProperty] decimal largestWinDay;
    [ObservableProperty] int     tradingDays;
    [ObservableProperty] int     winningDays;
    [ObservableProperty] int     losingDays;

    // ── Calculated Results ───────────────────────────────────────
    [ObservableProperty] decimal consistencyPercent;
    [ObservableProperty] decimal remainingProfit;
    [ObservableProperty] decimal maxSafeToday;
    [ObservableProperty] decimal buffer;
    [ObservableProperty] decimal maxWithdrawable;
    [ObservableProperty] decimal consistencyProgress;

    // ── Display Text ─────────────────────────────────────────────
    [ObservableProperty] string trafficLightText  = "🔴 RED – KEEP TRADING";
    [ObservableProperty] string trafficLightColor = "#FF5555";
    [ObservableProperty] string eligibilityText   = "❌ NOT YET ELIGIBLE";
    [ObservableProperty] string eligibilityColor  = "#FF5555";
    [ObservableProperty] string eligibilityBg     = "#2A1A1A";
    [ObservableProperty] string consistencyColor  = "#FF5555";
    [ObservableProperty] string netProfitColor    = "#F8F8F2";
    [ObservableProperty] string bufferColor       = "#F8F8F2";

    // ── Payout checklist ────────────────────────────────────────
    [ObservableProperty] string profitTargetMet = "❌";
    [ObservableProperty] string minDaysMet      = "❌";
    [ObservableProperty] string bufferMet       = "❌";

    // ── Stats lists ─────────────────────────────────────────────
    public ObservableCollection<StatRow> PerformanceStats { get; } = new();
    public ObservableCollection<StatRow> DrawdownStats    { get; } = new();
    public ObservableCollection<StatRow> CycleStats       { get; } = new();

    public MainViewModel()
    {
        // Load sample data on startup
        LoadSampleData();
        Recalculate();
    }

    private void LoadSampleData()
    {
        // Example from spec: Net=$857, LargestDay=$526
        CurrentBalance = StartingBalance + 857m;
        NetProfit      = 857m;
        LargestWinDay  = 526m;
        TradingDays    = 7;
        WinningDays    = 5;
        LosingDays     = 2;
    }

    partial void OnNetProfitChanged(decimal value) => Recalculate();
    partial void OnLargestWinDayChanged(decimal value) => Recalculate();
    partial void OnTradingDaysChanged(int value) => Recalculate();

    public void Recalculate()
    {
        // Consistency
        var con = ConsistencyEngine.Calculate(NetProfit, LargestWinDay);
        ConsistencyPercent  = con.ConsistencyPercent;
        RemainingProfit     = con.RemainingProfitNeeded;
        MaxSafeToday        = con.MaxSafeProfitToday;
        ConsistencyProgress = Math.Min(100m, Math.Max(0m, 100m - ConsistencyPercent));

        // Drawdown
        var dd = DrawdownEngine.Calculate(StartingBalance, CurrentBalance, MaxDrawdown, DailyDrawdown);
        Buffer = dd.Buffer;

        // Payout
        var pay = PayoutEngine.Calculate(NetProfit, ProfitTarget, Buffer,
            RequiredBuffer, TradingDays, MinTradingDays, ConsistencyPercent);
        MaxWithdrawable = pay.MaxWithdrawable;

        // Colors & text
        ConsistencyColor = con.ConsistencyPercent < 50m ? "#50FA7B" :
                           con.ConsistencyPercent < 65m ? "#F1FA8C" : "#FF5555";
        NetProfitColor = NetProfit >= 0 ? "#50FA7B" : "#FF5555";
        BufferColor    = Buffer >= RequiredBuffer ? "#50FA7B" : "#FF5555";

        ProfitTargetMet = pay.MeetsProfitTarget ? "✅" : "❌";
        MinDaysMet      = pay.MeetsTradingDays  ? "✅" : "❌";
        BufferMet       = pay.MeetsBuffer       ? "✅" : "❌";

        if (pay.IsEligible)
        {
            EligibilityText  = "✅ ELIGIBLE – REQUEST PAYOUT";
            EligibilityColor = "#50FA7B";
            EligibilityBg    = "#1A2A1A";
            TrafficLightText  = "🟢 GREEN – ELIGIBLE";
            TrafficLightColor = "#50FA7B";
        }
        else if (pay.MeetsProfitTarget && pay.MeetsTradingDays && pay.MeetsBuffer)
        {
            EligibilityText  = "🟡 ALMOST – Fix Consistency";
            EligibilityColor = "#F1FA8C";
            EligibilityBg    = "#2A2A1A";
            TrafficLightText  = "🟡 YELLOW – ALMOST";
            TrafficLightColor = "#F1FA8C";
        }
        else
        {
            EligibilityText  = "❌ NOT YET – Keep Trading";
            EligibilityColor = "#FF5555";
            EligibilityBg    = "#2A1A1A";
            TrafficLightText  = "🔴 RED – KEEP TRADING";
            TrafficLightColor = "#FF5555";
        }

        UpdateStatLists();
    }

    private void UpdateStatLists()
    {
        PerformanceStats.Clear();
        var total = WinningDays + LosingDays;
        var winRate = total > 0 ? (decimal)WinningDays / total * 100m : 0m;
        PerformanceStats.Add(new("Win Rate",     $"{winRate:N1}%"));
        PerformanceStats.Add(new("Winning Days", WinningDays.ToString()));
        PerformanceStats.Add(new("Losing Days",  LosingDays.ToString()));
        PerformanceStats.Add(new("Avg Daily P&L",NetProfit > 0 && TradingDays > 0 ?
            $"${NetProfit/TradingDays:N2}" : "$0.00"));

        DrawdownStats.Clear();
        DrawdownStats.Add(new("Max Drawdown",  $"${MaxDrawdown:N0}"));
        DrawdownStats.Add(new("Daily Limit",   $"${DailyDrawdown:N0}"));
        DrawdownStats.Add(new("Buffer Needed", $"${RequiredBuffer:N0}"));
        DrawdownStats.Add(new("Current Buffer",$"${Buffer:N2}"));

        CycleStats.Clear();
        CycleStats.Add(new("Net Profit",   $"${NetProfit:N2}"));
        CycleStats.Add(new("Profit Target",$"${ProfitTarget:N0}"));
        CycleStats.Add(new("Remaining",    $"${Math.Max(0,ProfitTarget-NetProfit):N2}"));
        CycleStats.Add(new("Trading Days", $"{TradingDays} / {MinTradingDays}"));
    }

    // ── Commands ─────────────────────────────────────────────────
    [RelayCommand]
    private void ImportCsv()
    {
        // TODO: Open file dialog, call CsvImporter, refresh data
    }

    [RelayCommand]
    private void NewPayoutCycle()
    {
        // TODO: Save current cycle to DB, reset
    }

    [RelayCommand]
    private void ExportExcel()
    {
        // TODO: Use ClosedXML to export report
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // TODO: Open settings window
    }
}

public record StatRow(string Label, string Value);