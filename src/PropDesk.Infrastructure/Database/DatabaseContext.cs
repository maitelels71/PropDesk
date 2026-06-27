using Microsoft.Data.Sqlite;
using PropDesk.Domain.Models;

namespace PropDesk.Infrastructure.Database;

/// <summary>
/// SQLite database context – creates tables on first run.
/// Connection string: Data Source=propdesk.db
/// </summary>
public class DatabaseContext : IDisposable
{
    private readonly SqliteConnection _conn;

    public DatabaseContext(string dbPath = "propdesk.db")
    {
        _conn = new SqliteConnection($"Data Source={dbPath}");
        _conn.Open();
        InitializeSchema();
    }

    private void InitializeSchema()
    {
        var sql = """
            CREATE TABLE IF NOT EXISTS Accounts (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Name            TEXT    NOT NULL,
                PropFirm        TEXT    NOT NULL DEFAULT "MyFundedFutures",
                AccountType     TEXT    NOT NULL DEFAULT "Sim Funded",
                AccountSize     REAL    NOT NULL,
                StartingBalance REAL    NOT NULL,
                ProfitTarget    REAL    NOT NULL,
                MaxDrawdown     REAL    NOT NULL,
                DailyDrawdown   REAL    NOT NULL,
                RequiredBuffer  REAL    NOT NULL,
                MinTradingDays  INTEGER NOT NULL DEFAULT 8,
                IsActive        INTEGER NOT NULL DEFAULT 1,
                CreatedAt       TEXT    NOT NULL DEFAULT (datetime("now"))
            );

            CREATE TABLE IF NOT EXISTS Trades (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId   INTEGER NOT NULL REFERENCES Accounts(Id),
                Date        TEXT    NOT NULL,
                Instrument  TEXT    NOT NULL,
                Direction   TEXT    NOT NULL,
                Contracts   INTEGER NOT NULL DEFAULT 1,
                EntryPrice  REAL    NOT NULL DEFAULT 0,
                ExitPrice   REAL    NOT NULL DEFAULT 0,
                GrossProfit REAL    NOT NULL DEFAULT 0,
                Commission  REAL    NOT NULL DEFAULT 0,
                Fees        REAL    NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS TradingDays (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId     INTEGER NOT NULL REFERENCES Accounts(Id),
                Date          TEXT    NOT NULL,
                DailyProfit   REAL    NOT NULL DEFAULT 0,
                TotalTrades   INTEGER NOT NULL DEFAULT 0,
                WinningTrades INTEGER NOT NULL DEFAULT 0,
                LosingTrades  INTEGER NOT NULL DEFAULT 0,
                Notes         TEXT,
                UNIQUE(AccountId, Date)
            );

            CREATE TABLE IF NOT EXISTS PayoutCycles (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                AccountId       INTEGER NOT NULL REFERENCES Accounts(Id),
                StartDate       TEXT    NOT NULL,
                EndDate         TEXT,
                StartingBalance REAL    NOT NULL,
                EndingBalance   REAL    NOT NULL DEFAULT 0,
                NetProfit       REAL    NOT NULL DEFAULT 0,
                LargestWinDay   REAL    NOT NULL DEFAULT 0,
                ConsistencyPct  REAL    NOT NULL DEFAULT 0,
                AmountPaid      REAL    NOT NULL DEFAULT 0,
                Status          TEXT    NOT NULL DEFAULT "Active",
                Notes           TEXT
            );

            CREATE TABLE IF NOT EXISTS Settings (
                Key   TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );
            """;

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public SqliteConnection Connection => _conn;

    public void Dispose() => _conn.Dispose();
}
