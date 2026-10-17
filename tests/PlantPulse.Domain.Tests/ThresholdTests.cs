using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Tests;

public class ThresholdTests
{
    private static readonly Threshold Above = new(95, ThresholdDirection.Above, 3);

    private static readonly Threshold Below = new(10, ThresholdDirection.Below, 2);

    [Theory]
    [InlineData(96, true)]
    [InlineData(95, false)]
    [InlineData(80, false)]
    public void Above_IsBreachedBy(double value, bool expected)
        => Assert.Equal(expected, Above.IsBreachedBy(value));

    [Theory]
    [InlineData(94, false)]
    [InlineData(92, true)]
    [InlineData(91, true)]
    public void Above_IsRecoveredBy_uses_hysteresis(double value, bool expected)
        => Assert.Equal(expected, Above.IsRecoveredBy(value));

    [Theory]
    [InlineData(9, true)]
    [InlineData(10, false)]
    [InlineData(30, false)]
    public void Below_IsBreachedBy(double value, bool expected)
        => Assert.Equal(expected, Below.IsBreachedBy(value));

    [Theory]
    [InlineData(11, false)]
    [InlineData(12, true)]
    [InlineData(40, true)]
    public void Below_IsRecoveredBy_uses_hysteresis(double value, bool expected)
        => Assert.Equal(expected, Below.IsRecoveredBy(value));

    [Fact]
    public void Negative_hysteresis_is_rejected()
        => Assert.Throws<DomainException>(() => new Threshold(95, ThresholdDirection.Above, -1));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Non_finite_limit_is_rejected(double limit)
        => Assert.Throws<DomainException>(() => new Threshold(limit, ThresholdDirection.Above, 1));

    [Fact]
    public void Thresholds_with_same_values_are_equal()
        => Assert.Equal(new Threshold(95, ThresholdDirection.Above, 3), new Threshold(95, ThresholdDirection.Above, 3));
}
