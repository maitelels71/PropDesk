using PropDesk.Core.Engines;

namespace PropDesk.Tests;

public class PayoutEngineTests
{
    [Fact]
    public void Eligible_WhenAllConditionsMet()
    {
        var result = PayoutEngine.Calculate(
            netProfit: 1500m,
            profitTarget: 1000m,
            currentBuffer: 3000m,
            requiredBuffer: 2500m,
            tradingDays: 10,
            minTradingDays: 8,
            consistencyPercent: 45m
        );

        Assert.True(result.IsEligible);
        Assert.Equal("Green", result.TrafficLight);
        Assert.True(result.MaxWithdrawable > 0);
    }

    [Fact]
    public void NotEligible_WhenConsistencyTooHigh()
    {
        var result = PayoutEngine.Calculate(
            netProfit: 1500m,
            profitTarget: 1000m,
            currentBuffer: 3000m,
            requiredBuffer: 2500m,
            tradingDays: 10,
            minTradingDays: 8,
            consistencyPercent: 65m // too high
        );

        Assert.False(result.IsEligible);
        Assert.False(result.MeetsConsistency);
    }
}