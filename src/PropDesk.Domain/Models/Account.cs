namespace PropDesk.Domain.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PropFirm { get; set; } = "MyFundedFutures";
    public string AccountType { get; set; } = "Sim Funded"; // Evaluation, Sim Funded, Live Funded
    public decimal AccountSize { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal ProfitTarget { get; set; }
    public decimal MaxDrawdown { get; set; }
    public decimal DailyDrawdownLimit { get; set; }
    public decimal RequiredBuffer { get; set; }
    public int MinTradingDays { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}