using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PropDesk.Domain.Models;

namespace PropDesk.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public Action? CloseAction { get; set; }

    [ObservableProperty] string  propFirm       = "MyFundedFutures";
    [ObservableProperty] string  accountName    = "My Account";
    [ObservableProperty] string  accountType    = "Sim Funded";
    [ObservableProperty] decimal accountSize    = 50_000m;
    [ObservableProperty] decimal startingBalance= 50_000m;
    [ObservableProperty] decimal profitTarget   = 3_000m;
    [ObservableProperty] decimal maxDrawdown    = 2_500m;
    [ObservableProperty] decimal dailyDrawdown  = 1_500m;
    [ObservableProperty] decimal requiredBuffer = 2_500m;
    [ObservableProperty] int     minTradingDays = 8;

    public Account ToAccount() => new()
    {
        Name            = AccountName,
        PropFirm        = PropFirm,
        AccountType     = AccountType,
        AccountSize     = AccountSize,
        StartingBalance = StartingBalance,
        ProfitTarget    = ProfitTarget,
        MaxDrawdown     = MaxDrawdown,
        DailyDrawdown   = DailyDrawdown,
        RequiredBuffer  = RequiredBuffer,
        MinTradingDays  = MinTradingDays,
    };

    public void LoadFromAccount(Account a)
    {
        PropFirm        = a.PropFirm;
        AccountName     = a.Name;
        AccountType     = a.AccountType;
        AccountSize     = a.AccountSize;
        StartingBalance = a.StartingBalance;
        ProfitTarget    = a.ProfitTarget;
        MaxDrawdown     = a.MaxDrawdown;
        DailyDrawdown   = a.DailyDrawdown;
        RequiredBuffer  = a.RequiredBuffer;
        MinTradingDays  = a.MinTradingDays;
    }

    [RelayCommand]
    private void Save() => CloseAction?.Invoke();
}