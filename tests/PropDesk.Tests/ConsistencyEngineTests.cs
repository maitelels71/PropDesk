using PropDesk.Core.Engines;

namespace PropDesk.Tests;

public class ConsistencyEngineTests
{
    [Fact]
    public void NotEligible_WhenLargestDayIsMore50Percent()
    {
        // Arrange: Day=$526, Net=$857 → 61.38% → NOT eligible
        var result = ConsistencyEngine.Calculate(857m, 526m);

        Assert.False(result.IsEligible);
        Assert.Equal(61.38m, result.ConsistencyPercent);
    }

    [Fact]
    public void Eligible_WhenConsistencyBelow50Percent()
    {
        // Arrange: Day=$526, Net=$1057 → 49.76% → ELIGIBLE
        var result = ConsistencyEngine.Calculate(1057m, 526m);

        Assert.True(result.IsEligible);
        Assert.True(result.ConsistencyPercent < 50m);
    }

    [Fact]
    public void RemainingProfit_CalculatedCorrectly()
    {
        // Need: 526/0.50 - 857 = 1052 - 857 = $195 more
        var result = ConsistencyEngine.Calculate(857m, 526m);

        Assert.Equal(195m, result.RemainingProfitNeeded);
    }

    [Fact]
    public void ZeroNetProfit_ReturnsNotEligible()
    {
        var result = ConsistencyEngine.Calculate(0m, 500m);
        Assert.False(result.IsEligible);
    }

    [Fact]
    public void ProjectedConsistency_CalculatesCorrectly()
    {
        // Net=$857, Largest=$526, Adding $200 → Net=$1057 → 49.76%
        var projected = ConsistencyEngine.ProjectedConsistency(857m, 526m, 200m);
        Assert.True(projected < 50m);
    }
}