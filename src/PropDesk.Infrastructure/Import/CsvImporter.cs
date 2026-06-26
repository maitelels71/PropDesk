using PropDesk.Domain.Models;

namespace PropDesk.Infrastructure.Import;

/// <summary>
/// Imports trades from MyFundedFutures CSV export.
/// CSV columns: Date, Instrument, Direction, Contracts, Entry, Exit, GrossProfit, Commission, Fees
/// </summary>
public static class CsvImporter
{
    public static ImportResult ImportFromFile(string filePath, int accountId)
    {
        var result = new ImportResult();

        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                result.Error = "File is empty or has no data rows.";
                return result;
            }

            // Parse header
            var headers = lines[0].Split(",").Select(h => h.Trim().ToLower()).ToArray();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(",");
                if (values.Length < 2) continue;

                try
                {
                    var trade = ParseTrade(headers, values, accountId);
                    result.Trades.Add(trade);
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"Row {i + 1}: {ex.Message}");
                }
            }

            result.Success = true;
            result.TotalImported = result.Trades.Count;
        }
        catch (Exception ex)
        {
            result.Error = ex.Message;
        }

        return result;
    }

    private static Trade ParseTrade(string[] headers, string[] values, int accountId)
    {
        T Get<T>(string col) where T : struct
        {
            var idx = Array.IndexOf(headers, col.ToLower());
            if (idx < 0 || idx >= values.Length) return default;
            var val = values[idx].Trim().Replace("$", "").Replace(",", "");
            return (T)Convert.ChangeType(val, typeof(T));
        }

        string GetStr(string col)
        {
            var idx = Array.IndexOf(headers, col.ToLower());
            return idx >= 0 && idx < values.Length ? values[idx].Trim() : string.Empty;
        }

        return new Trade
        {
            AccountId = accountId,
            Date = DateTime.Parse(GetStr("date")),
            Instrument = GetStr("instrument"),
            Direction = GetStr("direction"),
            Contracts = Get<int>("contracts"),
            EntryPrice = Get<decimal>("entry"),
            ExitPrice = Get<decimal>("exit"),
            GrossProfit = Get<decimal>("grossprofit"),
            Commission = Get<decimal>("commission"),
            Fees = Get<decimal>("fees")
        };
    }
}

public class ImportResult
{
    public bool Success { get; set; }
    public int TotalImported { get; set; }
    public List<Trade> Trades { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Error { get; set; }
}