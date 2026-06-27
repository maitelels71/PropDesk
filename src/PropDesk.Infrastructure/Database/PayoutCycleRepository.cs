using Microsoft.Data.Sqlite;
using PropDesk.Domain.Models;

namespace PropDesk.Infrastructure.Database;

public class PayoutCycleRepository(DatabaseContext db)
{
    public List<PayoutCycle> GetByAccount(int accountId)
    {
        var list = new List<PayoutCycle>();
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM PayoutCycles WHERE AccountId=$aid ORDER BY StartDate DESC";
        cmd.Parameters.AddWithValue("$aid", accountId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public int Insert(PayoutCycle c)
    {
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO PayoutCycles
            (AccountId,StartDate,EndDate,StartingBalance,EndingBalance,NetProfit,
             LargestWinDay,ConsistencyPct,AmountPaid,Status,Notes)
            VALUES ($aid,$sd,$ed,$sb,$eb,$np,$lwd,$cp,$ap,$st,$nt);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$aid", c.AccountId);
        cmd.Parameters.AddWithValue("$sd",  c.StartDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$ed",  c.EndDate?.ToString("yyyy-MM-dd") ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("$sb",  (double)c.StartingBalance);
        cmd.Parameters.AddWithValue("$eb",  (double)c.EndingBalance);
        cmd.Parameters.AddWithValue("$np",  (double)c.NetProfit);
        cmd.Parameters.AddWithValue("$lwd", (double)c.LargestWinDay);
        cmd.Parameters.AddWithValue("$cp",  (double)c.ConsistencyPercent);
        cmd.Parameters.AddWithValue("$ap",  (double)c.AmountPaid);
        cmd.Parameters.AddWithValue("$st",  c.Status);
        cmd.Parameters.AddWithValue("$nt",  c.Notes ?? (object)DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void UpdateStatus(int id, string status, decimal amountPaid)
    {
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "UPDATE PayoutCycles SET Status=$st, AmountPaid=$ap, EndDate=$ed WHERE Id=$id";
        cmd.Parameters.AddWithValue("$st",  status);
        cmd.Parameters.AddWithValue("$ap",  (double)amountPaid);
        cmd.Parameters.AddWithValue("$ed",  DateTime.Now.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$id",  id);
        cmd.ExecuteNonQuery();
    }

    private static PayoutCycle Map(SqliteDataReader r) => new()
    {
        Id              = r.GetInt32(r.GetOrdinal("Id")),
        AccountId       = r.GetInt32(r.GetOrdinal("AccountId")),
        StartDate       = DateTime.Parse(r.GetString(r.GetOrdinal("StartDate"))),
        EndDate         = r.IsDBNull(r.GetOrdinal("EndDate")) ? null : DateTime.Parse(r.GetString(r.GetOrdinal("EndDate"))),
        StartingBalance = (decimal)r.GetDouble(r.GetOrdinal("StartingBalance")),
        EndingBalance   = (decimal)r.GetDouble(r.GetOrdinal("EndingBalance")),
        NetProfit       = (decimal)r.GetDouble(r.GetOrdinal("NetProfit")),
        LargestWinDay   = (decimal)r.GetDouble(r.GetOrdinal("LargestWinDay")),
        ConsistencyPercent = (decimal)r.GetDouble(r.GetOrdinal("ConsistencyPct")),
        AmountPaid      = (decimal)r.GetDouble(r.GetOrdinal("AmountPaid")),
        Status          = r.GetString(r.GetOrdinal("Status")),
        Notes           = r.IsDBNull(r.GetOrdinal("Notes")) ? null : r.GetString(r.GetOrdinal("Notes")),
    };
}