using Microsoft.Data.Sqlite;
using PropDesk.Domain.Models;

namespace PropDesk.Infrastructure.Database;

public class TradeRepository(DatabaseContext db)
{
    public List<Trade> GetByAccount(int accountId)
    {
        var list = new List<Trade>();
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Trades WHERE AccountId=$aid ORDER BY Date";
        cmd.Parameters.AddWithValue("$aid", accountId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public void InsertBatch(IEnumerable<Trade> trades)
    {
        using var tx = db.Connection.BeginTransaction();
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Trades (AccountId,Date,Instrument,Direction,Contracts,
                EntryPrice,ExitPrice,GrossProfit,Commission,Fees)
            VALUES ($aid,$dt,$inst,$dir,$cnt,$ep,$xp,$gp,$comm,$fees)
            """;
        foreach (var t in trades)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("$aid",  t.AccountId);
            cmd.Parameters.AddWithValue("$dt",   t.Date.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("$inst", t.Instrument);
            cmd.Parameters.AddWithValue("$dir",  t.Direction);
            cmd.Parameters.AddWithValue("$cnt",  t.Contracts);
            cmd.Parameters.AddWithValue("$ep",   (double)t.EntryPrice);
            cmd.Parameters.AddWithValue("$xp",   (double)t.ExitPrice);
            cmd.Parameters.AddWithValue("$gp",   (double)t.GrossProfit);
            cmd.Parameters.AddWithValue("$comm", (double)t.Commission);
            cmd.Parameters.AddWithValue("$fees", (double)t.Fees);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    /// Group trades by date and return daily P&L
    public Dictionary<DateTime, decimal> GetDailyPnL(int accountId)
    {
        var trades = GetByAccount(accountId);
        return trades
            .GroupBy(t => t.Date.Date)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.NetProfit));
    }

    private static Trade Map(SqliteDataReader r) => new()
    {
        Id          = r.GetInt32(r.GetOrdinal("Id")),
        AccountId   = r.GetInt32(r.GetOrdinal("AccountId")),
        Date        = DateTime.Parse(r.GetString(r.GetOrdinal("Date"))),
        Instrument  = r.GetString(r.GetOrdinal("Instrument")),
        Direction   = r.GetString(r.GetOrdinal("Direction")),
        Contracts   = r.GetInt32(r.GetOrdinal("Contracts")),
        EntryPrice  = (decimal)r.GetDouble(r.GetOrdinal("EntryPrice")),
        ExitPrice   = (decimal)r.GetDouble(r.GetOrdinal("ExitPrice")),
        GrossProfit = (decimal)r.GetDouble(r.GetOrdinal("GrossProfit")),
        Commission  = (decimal)r.GetDouble(r.GetOrdinal("Commission")),
        Fees        = (decimal)r.GetDouble(r.GetOrdinal("Fees")),
    };
}
