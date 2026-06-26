namespace PropDesk.Domain.Models;

public class Trade
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime Date { get; set; }
    public string Instrument { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty; // Long / Short
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public int Contracts { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal Commission { get; set; }
    public decimal Fees { get; set; }
    public decimal NetProfit => GrossProfit - Commission - Fees;
}