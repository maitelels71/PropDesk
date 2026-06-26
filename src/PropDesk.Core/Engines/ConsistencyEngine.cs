namespace PropDesk.Core.Engines;

/// <summary>
/// Calculates the MyFundedFutures 50% Consistency Rule.
/// No single day can represent more than 50% of total net profit.
/// </summary>
public static class ConsistencyEngine
{
    public const decimal ConsistencyLimit = 0.50m;

    public static ConsistencyResult Calculate(decimal netProfit, decimal largestWinningDay)
    {
        if (netProfit <= 0)
            return new ConsistencyResult { IsEligible = false, ConsistencyPercent = 100m };

        var consistencyPercent = (largestWinningDay / netProfit) * 100m;
        var isEligible = consistencyPercent < (ConsistencyLimit * 100m);

        // How much more profit needed to become eligible
        // LargestDay / (NetProfit + X) = 0.50  =>  X = LargestDay/0.50 - NetProfit
        var remainingProfit = isEligible ? 0m :
            Math.Max(0m, (largestWinningDay / ConsistencyLimit) - netProfit);

        // Max safe profit today without breaking consistency
        // (LargestDay) / (NetProfit + TodayProfit) < 0.50
        // TodayProfit < LargestDay/0.50 - NetProfit
        var maxSafeProfitToday = Math.Max(0m, (largestWinningDay / ConsistencyLimit) - netProfit);

        return new ConsistencyResult
        {
            NetProfit = netProfit,
            LargestWinningDay = largestWinningDay,
            ConsistencyPercent = Math.Round(consistencyPercent, 2),
            IsEligible = isEligible,
            RemainingProfitNeeded = Math.Round(remainingProfit, 2),
            MaxSafeProfitToday = Math.Round(maxSafeProfitToday, 2)
        };
    }

    /// <summary>
    /// What would consistency be if we added X profit today?
    /// </summary>
    public static decimal ProjectedConsistency(decimal netProfit, decimal largestWinningDay, decimal additionalProfit)
    {
        var projectedNet = netProfit + additionalProfit;
        if (projectedNet <= 0) return 100m;
        return Math.Round((largestWinningDay / projectedNet) * 100m, 2);
    }
}

public class ConsistencyResult
{
    public decimal NetProfit { get; set; }
    public decimal LargestWinningDay { get; set; }
    public decimal ConsistencyPercent { get; set; }
    public bool IsEligible { get; set; }
    public decimal RemainingProfitNeeded { get; set; }
    public decimal MaxSafeProfitToday { get; set; }
    public string StatusLabel => IsEligible ? "ELIGIBLE ✅" : "NOT ELIGIBLE ❌";
    public string StatusColor => IsEligible ? "Green" : ConsistencyPercent < 60m ? "Yellow" : "Red";
}