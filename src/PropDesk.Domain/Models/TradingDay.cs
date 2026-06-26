namespace PropDesk.Domain.Models;

public class TradingDay
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal DailyProfit { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public bool IsManualEntry { get; set; }
    public string? Notes { get; set; }
}