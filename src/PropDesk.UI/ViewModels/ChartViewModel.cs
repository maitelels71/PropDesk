using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace PropDesk.UI.ViewModels;

public partial class ChartViewModel : ObservableObject
{
    private List<(DateTime Date, decimal DailyPnL, decimal NetPnL, decimal StartingBalance)> _data = new();

    // ── Chart Series ─────────────────────────────────────────────
    [ObservableProperty] ISeries[]  chartSeries  = Array.Empty<ISeries>();
    [ObservableProperty] Axis[]     xAxes        = DefaultXAxes();
    [ObservableProperty] Axis[]     yAxes        = DefaultYAxes();

    // ── Tab colors ───────────────────────────────────────────────
    [ObservableProperty] string equityTabColor      = "#50FA7B";
    [ObservableProperty] string dailyTabColor       = "#44475A";
    [ObservableProperty] string consistencyTabColor = "#44475A";

    // ── Mini stats ───────────────────────────────────────────────
    [ObservableProperty] decimal bestDay;
    [ObservableProperty] decimal worstDay;
    [ObservableProperty] decimal avgDay;
    [ObservableProperty] decimal chartNetPnL;

    private string _activeChart = "equity";

    public void LoadData(IEnumerable<(DateTime Date, decimal DailyPnL)> dailyData, decimal startingBalance)
    {
        var sorted = dailyData.OrderBy(d => d.Date).ToList();
        decimal running = startingBalance;
        _data = sorted.Select(d =>
        {
            running += d.DailyPnL;
            return (d.Date, d.DailyPnL, running, startingBalance);
        }).ToList();

        // Mini stats
        var pnls = sorted.Select(d => d.DailyPnL).ToList();
        BestDay     = pnls.Any() ? pnls.Max() : 0m;
        WorstDay    = pnls.Any() ? pnls.Min() : 0m;
        AvgDay      = pnls.Any() ? pnls.Average() : 0m;
        ChartNetPnL = pnls.Sum();

        // Load sample data if empty
        if (!_data.Any()) LoadSampleData(startingBalance);

        SwitchChart(_activeChart);
    }

    private void LoadSampleData(decimal startingBalance)
    {
        var rng  = new Random(42);
        var base_  = DateTime.Today.AddDays(-20);
        decimal net = startingBalance;
        _data = Enumerable.Range(0, 20).Select(i =>
        {
            var daily = (decimal)(rng.NextDouble() * 600 - 150);
            net += daily;
            return (base_.AddDays(i), daily, net, startingBalance);
        }).ToList();
        var pnls = _data.Select(d => d.DailyPnL).ToList();
        BestDay = pnls.Max(); WorstDay = pnls.Min();
        AvgDay = pnls.Average(); ChartNetPnL = pnls.Sum();
    }

    [RelayCommand]
    private void SwitchChart(string chart)
    {
        _activeChart = chart;
        EquityTabColor      = chart == "equity"      ? "#50FA7B" : "#44475A";
        DailyTabColor       = chart == "daily"       ? "#4C9BE8" : "#44475A";
        ConsistencyTabColor = chart == "consistency" ? "#F1FA8C" : "#44475A";

        switch (chart)
        {
            case "equity":      BuildEquityChart();      break;
            case "daily":       BuildDailyChart();       break;
            case "consistency": BuildConsistencyChart(); break;
        }
    }

    private void BuildEquityChart()
    {
        if (!_data.Any()) return;
        var startBal = _data[0].StartingBalance;
        var values   = _data.Select(d => new ObservableValue((double)d.NetPnL)).ToArray();
        var baseline = _data.Select(_ => new ObservableValue((double)startBal)).ToArray();

        ChartSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Name   = "Equity",
                Values = values,
                Fill   = new SolidColorPaint(SKColor.Parse("#1A50FA7B")),
                Stroke = new SolidColorPaint(SKColor.Parse("#50FA7B")) { StrokeThickness = 2.5f },
                GeometrySize   = 6,
                GeometryFill   = new SolidColorPaint(SKColor.Parse("#50FA7B")),
                GeometryStroke = new SolidColorPaint(SKColor.Parse("#1E1E2E")) { StrokeThickness = 1.5f },
            },
            new LineSeries<ObservableValue>
            {
                Name   = "Starting Balance",
                Values = baseline,
                Fill   = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#6272A4")) { StrokeThickness = 1f, PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[]{6,4}) },
                GeometrySize = 0,
            }
        };
        SetXLabels();
        YAxes = new Axis[] { new() { Name = "Balance ($)", LabelsPaint = new SolidColorPaint(SKColor.Parse("#6272A4")), TextSize = 11 } };
    }

    private void BuildDailyChart()
    {
        if (!_data.Any()) return;
        var values = _data.Select(d =>
            new ObservableValue((double)d.DailyPnL)).ToArray();

        ChartSeries = new ISeries[]
        {
            new ColumnSeries<ObservableValue>
            {
                Name   = "Daily P&L",
                Values = values,
                Fill   = new SolidColorPaint(SKColor.Parse("#4C9BE8")),
                Rx = 4, Ry = 4,
            }
        };
        SetXLabels();
        YAxes = new Axis[] { new() { Name = "P&L ($)", LabelsPaint = new SolidColorPaint(SKColor.Parse("#6272A4")), TextSize = 11 } };
    }

    private void BuildConsistencyChart()
    {
        if (!_data.Any()) return;
        decimal largestDay = 0m;
        var values = _data.Select(d =>
        {
            largestDay = Math.Max(largestDay, d.DailyPnL);
            var net = d.NetPnL - d.StartingBalance;
            var pct = net > 0 ? (double)(largestDay / net * 100m) : 100d;
            return new ObservableValue(Math.Min(pct, 150));
        }).ToArray();

        var limit = _data.Select(_ => new ObservableValue(50)).ToArray();

        ChartSeries = new ISeries[]
        {
            new LineSeries<ObservableValue>
            {
                Name   = "Consistency %",
                Values = values,
                Fill   = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#F1FA8C")) { StrokeThickness = 2.5f },
                GeometrySize = 5,
                GeometryFill = new SolidColorPaint(SKColor.Parse("#F1FA8C")),
            },
            new LineSeries<ObservableValue>
            {
                Name   = "50% Limit",
                Values = limit,
                Fill   = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#FF5555")) { StrokeThickness = 1.5f, PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[]{6,4}) },
                GeometrySize = 0,
            }
        };
        SetXLabels();
        YAxes = new Axis[] { new() { Name = "Consistency %", LabelsPaint = new SolidColorPaint(SKColor.Parse("#6272A4")), TextSize = 11, MinLimit = 0, MaxLimit = 110 } };
    }

    private void SetXLabels()
    {
        var labels = _data.Select(d => d.Date.ToString("MM/dd")).ToArray();
        XAxes = new Axis[]
        {
            new()
            {
                Labels     = labels,
                LabelsPaint= new SolidColorPaint(SKColor.Parse("#6272A4")),
                TextSize   = 10,
                LabelsRotation = -30,
            }
        };
    }

    private static Axis[] DefaultXAxes() => new Axis[]
    {
        new() { LabelsPaint = new SolidColorPaint(SKColor.Parse("#6272A4")), TextSize = 10 }
    };

    private static Axis[] DefaultYAxes() => new Axis[]
    {
        new() { LabelsPaint = new SolidColorPaint(SKColor.Parse("#6272A4")), TextSize = 11 }
    };
}