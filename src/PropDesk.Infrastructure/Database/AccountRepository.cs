using Microsoft.Data.Sqlite;
using PropDesk.Domain.Models;

namespace PropDesk.Infrastructure.Database;

public class AccountRepository(DatabaseContext db)
{
    public List<Account> GetAll()
    {
        var list = new List<Account>();
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Accounts WHERE IsActive = 1 ORDER BY Name";
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Map(r));
        return list;
    }

    public Account? GetById(int id)
    {
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Accounts WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Map(r) : null;
    }

    public int Insert(Account a)
    {
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Accounts 
            (Name,PropFirm,AccountType,AccountSize,StartingBalance,ProfitTarget,
             MaxDrawdown,DailyDrawdown,RequiredBuffer,MinTradingDays)
            VALUES ($n,$pf,$at,$as,$sb,$pt,$md,$dd,$rb,$mtd);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$n",   a.Name);
        cmd.Parameters.AddWithValue("$pf",  a.PropFirm);
        cmd.Parameters.AddWithValue("$at",  a.AccountType);
        cmd.Parameters.AddWithValue("$as",  a.AccountSize);
        cmd.Parameters.AddWithValue("$sb",  a.StartingBalance);
        cmd.Parameters.AddWithValue("$pt",  a.ProfitTarget);
        cmd.Parameters.AddWithValue("$md",  a.MaxDrawdown);
        cmd.Parameters.AddWithValue("$dd",  a.DailyDrawdown);
        cmd.Parameters.AddWithValue("$rb",  a.RequiredBuffer);
        cmd.Parameters.AddWithValue("$mtd", a.MinTradingDays);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Account a)
    {
        using var cmd = db.Connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Accounts SET
                Name=        $n,  PropFirm=$pf, AccountType=$at,
                AccountSize= $as, ProfitTarget=$pt, MaxDrawdown=$md,
                DailyDrawdown=$dd, RequiredBuffer=$rb, MinTradingDays=$mtd
            WHERE Id = $id
            """;
        cmd.Parameters.AddWithValue("$id",  a.Id);
        cmd.Parameters.AddWithValue("$n",   a.Name);
        cmd.Parameters.AddWithValue("$pf",  a.PropFirm);
        cmd.Parameters.AddWithValue("$at",  a.AccountType);
        cmd.Parameters.AddWithValue("$as",  a.AccountSize);
        cmd.Parameters.AddWithValue("$pt",  a.ProfitTarget);
        cmd.Parameters.AddWithValue("$md",  a.MaxDrawdown);
        cmd.Parameters.AddWithValue("$dd",  a.DailyDrawdown);
        cmd.Parameters.AddWithValue("$rb",  a.RequiredBuffer);
        cmd.Parameters.AddWithValue("$mtd", a.MinTradingDays);
        cmd.ExecuteNonQuery();
    }

    private static Account Map(SqliteDataReader r) => new()
    {
        Id             = r.GetInt32(r.GetOrdinal("Id")),
        Name           = r.GetString(r.GetOrdinal("Name")),
        PropFirm       = r.GetString(r.GetOrdinal("PropFirm")),
        AccountType    = r.GetString(r.GetOrdinal("AccountType")),
        AccountSize    = (decimal)r.GetDouble(r.GetOrdinal("AccountSize")),
        StartingBalance= (decimal)r.GetDouble(r.GetOrdinal("StartingBalance")),
        ProfitTarget   = (decimal)r.GetDouble(r.GetOrdinal("ProfitTarget")),
        MaxDrawdown    = (decimal)r.GetDouble(r.GetOrdinal("MaxDrawdown")),
        DailyDrawdown  = (decimal)r.GetDouble(r.GetOrdinal("DailyDrawdown")),
        RequiredBuffer = (decimal)r.GetDouble(r.GetOrdinal("RequiredBuffer")),
        MinTradingDays = r.GetInt32(r.GetOrdinal("MinTradingDays")),
        IsActive       = r.GetInt32(r.GetOrdinal("IsActive")) == 1,
    };
}
