namespace PropDesk.Domain.Models;

public class PayoutCycle
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal NetProfit { get; set; }
    public decimal LargestWinningDay { get; set; }
    public decimal ConsistencyPercent { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = "Active"; // Active, Paid, Skipped
    public string? Notes { get; set; }
}