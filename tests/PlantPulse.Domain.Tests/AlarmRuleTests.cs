using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Tests;

public class AlarmRuleTests
{
    private static readonly Threshold Limit = new(210, ThresholdDirection.Above, 5);

    [Fact]
    public void Zero_duration_is_rejected()
        => Assert.Throws<DomainException>(() =>
            new AlarmRule(Guid.NewGuid(), Limit, TimeSpan.Zero, AlarmSeverity.Warning));

    [Fact]
    public void Empty_device_is_rejected()
        => Assert.Throws<DomainException>(() =>
            new AlarmRule(Guid.Empty, Limit, TimeSpan.FromSeconds(30), AlarmSeverity.Warning));

    [Fact]
    public void Missing_threshold_is_rejected()
        => Assert.Throws<DomainException>(() =>
            new AlarmRule(Guid.NewGuid(), null!, TimeSpan.FromSeconds(30), AlarmSeverity.Warning));

    [Fact]
    public void Rule_can_be_disabled_and_enabled()
    {
        var rule = new AlarmRule(Guid.NewGuid(), Limit, TimeSpan.FromSeconds(30), AlarmSeverity.Warning);

        rule.Disable();
        Assert.False(rule.IsEnabled);

        rule.Enable();
        Assert.True(rule.IsEnabled);
    }
}
