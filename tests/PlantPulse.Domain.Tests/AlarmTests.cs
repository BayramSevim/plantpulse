using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Alarms.Events;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Tests;

public class AlarmTests
{
    private static readonly DateTime T0 = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    private static Alarm OpenAlarm()
    {
        var rule = new AlarmRule(Guid.NewGuid(), new Threshold(210, ThresholdDirection.Above, 5),
            TimeSpan.FromSeconds(30), AlarmSeverity.Critical);
        return new Alarm(rule, 226.8, T0);
    }

    [Fact]
    public void New_alarm_is_active_and_raises_AlarmRaised()
    {
        var alarm = OpenAlarm();

        Assert.Equal(AlarmStatus.Active, alarm.Status);
        var raised = Assert.IsType<AlarmRaised>(Assert.Single(alarm.DomainEvents));
        Assert.Equal(alarm.Id, raised.AlarmId);
        Assert.Equal(226.8, raised.TriggerValue);
    }

    [Fact]
    public void Clear_sets_fields_and_raises_AlarmCleared()
    {
        var alarm = OpenAlarm();
        alarm.ClearDomainEvents();

        alarm.Clear(204, T0.AddMinutes(5));

        Assert.Equal(AlarmStatus.Cleared, alarm.Status);
        Assert.Equal(204, alarm.ClearedValue);
        Assert.Equal(T0.AddMinutes(5), alarm.ClearedAt);
        Assert.IsType<AlarmCleared>(Assert.Single(alarm.DomainEvents));
    }

    [Fact]
    public void Cannot_clear_twice()
    {
        var alarm = OpenAlarm();
        alarm.Clear(204, T0.AddMinutes(1));

        Assert.Throws<DomainException>(() => alarm.Clear(200, T0.AddMinutes(2)));
    }

    [Fact]
    public void Cannot_clear_before_it_was_opened()
        => Assert.Throws<DomainException>(() => OpenAlarm().Clear(204, T0.AddSeconds(-1)));

    [Fact]
    public void Acknowledge_records_who_and_when()
    {
        var alarm = OpenAlarm();
        alarm.ClearDomainEvents();

        alarm.Acknowledge("  ahmet  ", T0.AddMinutes(1));

        Assert.Equal("ahmet", alarm.AcknowledgedBy);
        Assert.Equal(T0.AddMinutes(1), alarm.AcknowledgedAt);
        Assert.IsType<AlarmAcknowledged>(Assert.Single(alarm.DomainEvents));
    }

    [Fact]
    public void Cannot_acknowledge_twice()
    {
        var alarm = OpenAlarm();
        alarm.Acknowledge("ahmet", T0.AddMinutes(1));

        Assert.Throws<DomainException>(() => alarm.Acknowledge("mehmet", T0.AddMinutes(2)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Acknowledge_requires_a_name(string by)
        => Assert.Throws<DomainException>(() => OpenAlarm().Acknowledge(by, T0.AddMinutes(1)));

    [Fact]
    public void Cleared_alarm_can_still_be_acknowledged()
    {
        var alarm = OpenAlarm();
        alarm.Clear(204, T0.AddMinutes(5));

        alarm.Acknowledge("ahmet", T0.AddHours(5));

        Assert.Equal("ahmet", alarm.AcknowledgedBy);
    }

    [Fact]
    public void NaN_trigger_value_is_rejected()
    {
        var rule = new AlarmRule(Guid.NewGuid(), new Threshold(210, ThresholdDirection.Above, 5),
            TimeSpan.FromSeconds(30), AlarmSeverity.Critical);

        Assert.Throws<DomainException>(() => new Alarm(rule, double.NaN, T0));
    }
}
