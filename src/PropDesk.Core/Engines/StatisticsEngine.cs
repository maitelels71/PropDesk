namespace PropDesk.Core.Engines;

public static class StatisticsEngine
{
    public static TradingStatistics Calculate(IEnumerable<decimal> dailyProfits)
    {
        var profits = dailyProfits.ToList();
        if (!profits.Any()) return new TradingStatistics();

        var wins = profits.Where(p => p > 0).ToList();
        var losses = profits.Where(p => p < 0).ToList();

        var totalDays = profits.Count;
        var winDays = wins.Count;
        var lossDays = losses.Count;

        var grossProfit = wins.Sum();
        var grossLoss = Math.Abs(losses.Sum());

        var avgWin = wins.Any() ? wins.Average() : 0m;
        var avgLoss = losses.Any() ? Math.Abs(losses.Average()) : 0m;

        var profitFactor = grossLoss > 0 ? grossProfit / grossLoss : grossProfit > 0 ? 999m : 0m;
        var winRate = totalDays > 0 ? (decimal)winDays / totalDays * 100m : 0m;

        // Expectancy = (WinRate * AvgWin) - (LossRate * AvgLoss)
        var lossRate = 100m - winRate;
        var expectancy = (winRate / 100m * avgWin) - (lossRate / 100m * avgLoss);

        // Streaks
        var (winStreak, lossStreak) = CalculateStreaks(profits);

        return new TradingStatistics
        {
            TotalDays = totalDays,
            WinningDays = winDays,
            LosingDays = lossDays,
            WinRate = Math.Round(winRate, 2),
            LossRate = Math.Round(100m - winRate, 2),
            GrossProfit = Math.Round(grossProfit, 2),
            GrossLoss = Math.Round(grossLoss, 2),
            NetProfit = Math.Round(profits.Sum(), 2),
            LargestWin = wins.Any() ? wins.Max() : 0m,
            LargestLoss = losses.Any() ? Math.Abs(losses.Min()) : 0m,
            AverageWin = Math.Round(avgWin, 2),
            AverageLoss = Math.Round(avgLoss, 2),
            ProfitFactor = Math.Round(profitFactor, 2),
            Expectancy = Math.Round(expectancy, 2),
            MaxWinStreak = winStreak,
            MaxLossStreak = lossStreak,
            AverageDailyProfit = Math.Round(profits.Average(), 2)
        };
    }

    private static (int winStreak, int lossStreak) CalculateStreaks(List<decimal> profits)
    {
        int maxWin = 0, maxLoss = 0, curWin = 0, curLoss = 0;
        foreach (var p in profits)
        {
            if (p > 0) { curWin++; curLoss = 0; maxWin = Math.Max(maxWin, curWin); }
            else if (p < 0) { curLoss++; curWin = 0; maxLoss = Math.Max(maxLoss, curLoss); }
            else { curWin = 0; curLoss = 0; }
        }
        return (maxWin, maxLoss);
    }
}

public class TradingStatistics
{
    public int TotalDays { get; set; }
    public int WinningDays { get; set; }
    public int LosingDays { get; set; }
    public decimal WinRate { get; set; }
    public decimal LossRate { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal GrossLoss { get; set; }
    public decimal NetProfit { get; set; }
    public decimal LargestWin { get; set; }
    public decimal LargestLoss { get; set; }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal Expectancy { get; set; }
    public int MaxWinStreak { get; set; }
    public int MaxLossStreak { get; set; }
    public decimal AverageDailyProfit { get; set; }
}