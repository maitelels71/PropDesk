using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PropDesk.Core.Engines;
using PropDesk.Domain.Models;
using PropDesk.Infrastructure.Database;
using PropDesk.Infrastructure.Import;
using PropDesk.Reporting;
using PropDesk.UI.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace PropDesk.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseContext       _db;
    private readonly TradeRepository       _tradeRepo;
    private readonly AccountRepository     _accountRepo;
    private readonly PayoutCycleRepository _cycleRepo;
    private Account _account;

    [ObservableProperty] decimal startingBalance = 50_000m;
    [ObservableProperty] decimal profitTarget    = 3_000m;
    [ObservableProperty] decimal maxDrawdown     = 2_500m;
    [ObservableProperty] decimal dailyDrawdown   = 1_500m;
    [ObservableProperty] decimal requiredBuffer  = 2_500m;
    [ObservableProperty] int     minTradingDays  = 8;
    [ObservableProperty] decimal currentBalance;
    [ObservableProperty] decimal netProfit;
    [ObservableProperty] decimal largestWinDay;
    [ObservableProperty] int     tradingDays;
    [ObservableProperty] int     winningDays;
    [ObservableProperty] int     losingDays;
    [ObservableProperty] decimal consistencyPercent;
    [ObservableProperty] decimal remainingProfit;
    [ObservableProperty] decimal maxSafeToday;
    [ObservableProperty] decimal buffer;
    [ObservableProperty] decimal maxWithdrawable;
    [ObservableProperty] decimal consistencyProgress;
    [ObservableProperty] string trafficLightText  = "🔴 RED – KEEP TRADING";
    [ObservableProperty] string trafficLightColor = "#FF5555";
    [ObservableProperty] string eligibilityText   = "❌ NOT YET ELIGIBLE";
    [ObservableProperty] string eligibilityColor  = "#FF5555";
    [ObservableProperty] string eligibilityBg     = "#2A1A1A";
    [ObservableProperty] string consistencyColor  = "#FF5555";
    [ObservableProperty] string netProfitColor    = "#F8F8F2";
    [ObservableProperty] string bufferColor       = "#F8F8F2";
    [ObservableProperty] string profitTargetMet   = "❌";
    [ObservableProperty] string minDaysMet        = "❌";
    [ObservableProperty] string bufferMet         = "❌";
    [ObservableProperty] string statusMessage     = "Loading...";

    public ChartViewModel ChartVM { get; } = new();
    public ObservableCollection<StatRow> PerformanceStats { get; } = new();
    public ObservableCollection<StatRow> DrawdownStats    { get; } = new();
    public ObservableCollection<StatRow> CycleStats       { get; } = new();
    private Dictionary<DateTime, decimal> _dailyPnL = new();

    public MainViewModel()
    {
        _db          = new DatabaseContext();
        _tradeRepo   = new TradeRepository(_db);
        _accountRepo = new AccountRepository(_db);
        _cycleRepo   = new PayoutCycleRepository(_db);
        _account     = _accountRepo.GetAll().FirstOrDefault() ?? CreateDefaultAccount();
        LoadFromAccount(_account);
        RefreshFromDatabase();
    }

    private Account CreateDefaultAccount()
    {
        var a = new Account { Name="My Account", PropFirm="MyFundedFutures",
            AccountSize=50_000m, StartingBalance=50_000m, ProfitTarget=3_000m,
            MaxDrawdown=2_500m, DailyDrawdown=1_500m, RequiredBuffer=2_500m, MinTradingDays=8 };
        a.Id = _accountRepo.Insert(a);
        return a;
    }

    private void LoadFromAccount(Account a)
    {
        StartingBalance=a.StartingBalance; ProfitTarget=a.ProfitTarget;
        MaxDrawdown=a.MaxDrawdown; DailyDrawdown=a.DailyDrawdown;
        RequiredBuffer=a.RequiredBuffer; MinTradingDays=a.MinTradingDays;
    }

    public void RefreshFromDatabase()
    {
        _dailyPnL = _tradeRepo.GetDailyPnL(_account.Id);
        if (!_dailyPnL.Any()) { LoadSampleData(); return; }
        NetProfit=_dailyPnL.Values.Sum();
        LargestWinDay=_dailyPnL.Values.Where(v=>v>0).DefaultIfEmpty(0).Max();
        TradingDays=_dailyPnL.Count; WinningDays=_dailyPnL.Values.Count(v=>v>0); LosingDays=_dailyPnL.Values.Count(v=>v<0);
        CurrentBalance=StartingBalance+NetProfit;
        StatusMessage=$"Loaded {TradingDays} trading days";
        Recalculate(); RefreshChart();
    }

    private void LoadSampleData()
    {
        var base_=DateTime.Today.AddDays(-14);
        _dailyPnL=new Dictionary<DateTime,decimal>{{base_.AddDays(0),526m},{base_.AddDays(1),-80m},{base_.AddDays(2),210m},{base_.AddDays(3),150m},{base_.AddDays(4),-120m},{base_.AddDays(5),95m},{base_.AddDays(6),-24m}};
        NetProfit=_dailyPnL.Values.Sum(); LargestWinDay=_dailyPnL.Values.Max();
        TradingDays=_dailyPnL.Count; WinningDays=_dailyPnL.Values.Count(v=>v>0); LosingDays=_dailyPnL.Values.Count(v=>v<0);
        CurrentBalance=StartingBalance+NetProfit;
        StatusMessage="Demo data – import your CSV to get started";
        Recalculate(); RefreshChart();
    }

    private void RefreshChart() => ChartVM.LoadData(_dailyPnL.Select(kv=>(kv.Key,kv.Value)), StartingBalance);

    partial void OnNetProfitChanged(decimal v)     => Recalculate();
    partial void OnLargestWinDayChanged(decimal v) => Recalculate();
    partial void OnTradingDaysChanged(int v)       => Recalculate();

    public void Recalculate()
    {
        var con=ConsistencyEngine.Calculate(NetProfit,LargestWinDay);
        ConsistencyPercent=con.ConsistencyPercent; RemainingProfit=con.RemainingProfitNeeded;
        MaxSafeToday=con.MaxSafeProfitToday; ConsistencyProgress=Math.Min(100m,Math.Max(0m,100m-ConsistencyPercent));
        var dd=DrawdownEngine.Calculate(StartingBalance,CurrentBalance,MaxDrawdown,DailyDrawdown);
        Buffer=dd.Buffer;
        var pay=PayoutEngine.Calculate(NetProfit,ProfitTarget,Buffer,RequiredBuffer,TradingDays,MinTradingDays,ConsistencyPercent);
        MaxWithdrawable=pay.MaxWithdrawable;
        ConsistencyColor=con.ConsistencyPercent<50m?"#50FA7B":con.ConsistencyPercent<65m?"#F1FA8C":"#FF5555";
        NetProfitColor=NetProfit>=0?"#50FA7B":"#FF5555"; BufferColor=Buffer>=RequiredBuffer?"#50FA7B":"#FF5555";
        ProfitTargetMet=pay.MeetsProfitTarget?"✅":"❌"; MinDaysMet=pay.MeetsTradingDays?"✅":"❌"; BufferMet=pay.MeetsBuffer?"✅":"❌";
        if(pay.IsEligible){EligibilityText="✅ ELIGIBLE – REQUEST PAYOUT";EligibilityColor="#50FA7B";EligibilityBg="#1A2A1A";TrafficLightText="🟢 GREEN – ELIGIBLE";TrafficLightColor="#50FA7B";}
        else if(pay.MeetsProfitTarget&&pay.MeetsTradingDays&&pay.MeetsBuffer){EligibilityText="🟡 ALMOST – Fix Consistency";EligibilityColor="#F1FA8C";EligibilityBg="#2A2A1A";TrafficLightText="🟡 YELLOW – ALMOST";TrafficLightColor="#F1FA8C";}
        else{EligibilityText="❌ NOT YET – Keep Trading";EligibilityColor="#FF5555";EligibilityBg="#2A1A1A";TrafficLightText="🔴 RED – KEEP TRADING";TrafficLightColor="#FF5555";}
        UpdateStatLists();
    }

    private void UpdateStatLists()
    {
        PerformanceStats.Clear();
        var total=WinningDays+LosingDays; var wr=total>0?(decimal)WinningDays/total*100m:0m;
        PerformanceStats.Add(new("Win Rate",$"{wr:N1}%")); PerformanceStats.Add(new("Winning Days",WinningDays.ToString()));
        PerformanceStats.Add(new("Losing Days",LosingDays.ToString())); PerformanceStats.Add(new("Avg Daily",TradingDays>0?$"${NetProfit/TradingDays:N2}":"$0.00"));
        DrawdownStats.Clear();
        DrawdownStats.Add(new("Max Drawdown",$"${MaxDrawdown:N0}")); DrawdownStats.Add(new("Daily Limit",$"${DailyDrawdown:N0}"));
        DrawdownStats.Add(new("Buffer Needed",$"${RequiredBuffer:N0}")); DrawdownStats.Add(new("Current Buffer",$"${Buffer:N2}"));
        CycleStats.Clear();
        CycleStats.Add(new("Net Profit",$"${NetProfit:N2}")); CycleStats.Add(new("Target",$"${ProfitTarget:N0}"));
        CycleStats.Add(new("Remaining",$"${Math.Max(0,ProfitTarget-NetProfit):N2}")); CycleStats.Add(new("Days",$"{TradingDays}/{MinTradingDays}"));
    }

    [RelayCommand]
    private void ImportCsv()
    {
        var vm=new ImportViewModel(_account.Id); var win=new ImportWindow(vm);
        vm.OnImportComplete=trades=>{_tradeRepo.InsertBatch(trades);RefreshFromDatabase();StatusMessage=$"✅ Imported {trades.Count} trades";};
        win.Owner=Application.Current.MainWindow; win.ShowDialog();
    }

    [RelayCommand]
    private void OpenPayoutHistory()
    {
        var vm=new PayoutHistoryViewModel(_db,_account.Id);
        var win=new PayoutHistoryWindow(vm);
        win.Owner=Application.Current.MainWindow; win.ShowDialog();
    }

    [RelayCommand]
    private void ExportReport()
    {
        var dlg=new SaveFileDialog{Title="Export Report",Filter="HTML (*.html)|*.html",FileName=$"PropDesk_{DateTime.Now:yyyy-MM-dd}.html"};
        if(dlg.ShowDialog()!=true)return;
        var svc=new PdfReportService();
        svc.ExportDashboardReport(dlg.FileName,new DashboardReportData(_account.Name,NetProfit,ConsistencyPercent,LargestWinDay,Buffer,TradingDays,MinTradingDays,MaxWithdrawable>0));
        StatusMessage="✅ Report exported";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo{FileName=dlg.FileName,UseShellExecute=true});
    }

    [RelayCommand]
    private void NewPayoutCycle()
    {
        _cycleRepo.Insert(new PayoutCycle{AccountId=_account.Id,StartDate=DateTime.Today,StartingBalance=CurrentBalance,NetProfit=NetProfit,LargestWinDay=LargestWinDay,ConsistencyPercent=ConsistencyPercent,Status="Active"});
        StatusMessage=$"✅ New cycle started {DateTime.Today:MM/dd/yyyy}";
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var vm=new SettingsViewModel(); vm.LoadFromAccount(_account);
        var win=new SettingsWindow(vm); win.Owner=Application.Current.MainWindow;
        if(win.ShowDialog()==true){var u=vm.ToAccount();u.Id=_account.Id;_accountRepo.Update(u);_account=u;LoadFromAccount(u);Recalculate();RefreshChart();StatusMessage="⚙ Settings saved";}
    }
}

public record StatRow(string Label, string Value);
