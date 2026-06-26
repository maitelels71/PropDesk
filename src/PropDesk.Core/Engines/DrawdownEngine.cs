namespace PropDesk.Core.Engines;

public static class DrawdownEngine
{
    public static DrawdownResult Calculate(decimal startingBalance, decimal currentBalance, 
        decimal maxDrawdownLimit, decimal dailyDrawdownLimit)
    {
        var netProfit = currentBalance - startingBalance;
        
        // Trailing drawdown threshold
        var drawdownLevel = startingBalance - maxDrawdownLimit;
        var buffer = currentBalance - drawdownLevel;
        var bufferPercent = maxDrawdownLimit > 0 ? (buffer / maxDrawdownLimit) * 100m : 0m;

        // Daily drawdown
        var dailyDrawdownLevel = currentBalance - dailyDrawdownLimit;

        return new DrawdownResult
        {
            CurrentBalance = currentBalance,
            NetProfit = netProfit,
            DrawdownLevel = drawdownLevel,
            Buffer = buffer,
            BufferPercent = Math.Round(bufferPercent, 2),
            DailyDrawdownLevel = dailyDrawdownLevel,
            IsBufferSafe = buffer > 0,
            MaxDrawdownLimit = maxDrawdownLimit,
            DailyDrawdownLimit = dailyDrawdownLimit
        };
    }
}

public class DrawdownResult
{
    public decimal CurrentBalance { get; set; }
    public decimal NetProfit { get; set; }
    public decimal DrawdownLevel { get; set; }
    public decimal Buffer { get; set; }
    public decimal BufferPercent { get; set; }
    public decimal DailyDrawdownLevel { get; set; }
    public decimal MaxDrawdownLimit { get; set; }
    public decimal DailyDrawdownLimit { get; set; }
    public bool IsBufferSafe { get; set; }
}