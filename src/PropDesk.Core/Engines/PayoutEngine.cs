namespace PropDesk.Core.Engines;

public static class PayoutEngine
{
    public static PayoutResult Calculate(
        decimal netProfit,
        decimal profitTarget,
        decimal currentBuffer,
        decimal requiredBuffer,
        int tradingDays,
        int minTradingDays,
        decimal consistencyPercent,
        decimal maxWithdrawalPercent = 0.50m)
    {
        var meetsProfit = netProfit >= profitTarget;
        var meetsBuffer = currentBuffer >= requiredBuffer;
        var meetsDays = tradingDays >= minTradingDays;
        var meetsConsistency = consistencyPercent < 50m;

        var isEligible = meetsProfit && meetsBuffer && meetsDays && meetsConsistency;

        var maxWithdrawable = isEligible ? netProfit * maxWithdrawalPercent : 0m;

        return new PayoutResult
        {
            IsEligible = isEligible,
            MeetsProfitTarget = meetsProfit,
            MeetsBuffer = meetsBuffer,
            MeetsTradingDays = meetsDays,
            MeetsConsistency = meetsConsistency,
            MaxWithdrawable = Math.Round(maxWithdrawable, 2),
            RemainingProfit = Math.Max(0m, profitTarget - netProfit),
            RemainingDays = Math.Max(0, minTradingDays - tradingDays)
        };
    }
}

public class PayoutResult
{
    public bool IsEligible { get; set; }
    public bool MeetsProfitTarget { get; set; }
    public bool MeetsBuffer { get; set; }
    public bool MeetsTradingDays { get; set; }
    public bool MeetsConsistency { get; set; }
    public decimal MaxWithdrawable { get; set; }
    public decimal RemainingProfit { get; set; }
    public int RemainingDays { get; set; }
    public string TrafficLight => IsEligible ? "Green" : 
        (MeetsProfitTarget && MeetsBuffer && MeetsTradingDays) ? "Yellow" : "Red";
}