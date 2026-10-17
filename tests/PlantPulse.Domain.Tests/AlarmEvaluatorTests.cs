using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Alarms.RuleEngine;

namespace PlantPulse.Domain.Tests;

public class AlarmEvaluatorTests
{
    private static AlarmRule PressRule() => new(
        Guid.NewGuid(),
        new Threshold(210, ThresholdDirection.Above, 5),
        TimeSpan.FromSeconds(30),
        AlarmSeverity.Critical);

    private static readonly DateTime T0 = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void First_breach_starts_the_timer_without_alarm()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 215, T0, breachStartedAt: null, hasActiveAlarm: false);

        Assert.Equal(EvaluationOutcome.None, result.Outcome);
        Assert.Equal(T0, result.BreachStartedAt);
    }

    [Fact]
    public void Breach_shorter_than_duration_keeps_original_start()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 218, T0.AddSeconds(20), breachStartedAt: T0, hasActiveAlarm: false);

        Assert.Equal(EvaluationOutcome.None, result.Outcome);
        Assert.Equal(T0, result.BreachStartedAt);
    }

    [Fact]
    public void Breach_reaching_duration_opens_alarm()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 220, T0.AddSeconds(30), breachStartedAt: T0, hasActiveAlarm: false);

        Assert.Equal(EvaluationOutcome.OpenAlarm, result.Outcome);
    }

    [Fact]
    public void Value_back_under_limit_resets_the_timer()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 208, T0.AddSeconds(20), breachStartedAt: T0, hasActiveAlarm: false);

        Assert.Equal(EvaluationOutcome.None, result.Outcome);
        Assert.Null(result.BreachStartedAt);
    }

    [Fact]
    public void Active_alarm_stays_open_in_dead_zone()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 207, T0, breachStartedAt: null, hasActiveAlarm: true);

        Assert.Equal(EvaluationOutcome.None, result.Outcome);
    }

    [Fact]
    public void Active_alarm_clears_after_recovery()
    {
        var result = AlarmEvaluator.Evaluate(PressRule(), 204, T0, breachStartedAt: null, hasActiveAlarm: true);

        Assert.Equal(EvaluationOutcome.ClearAlarm, result.Outcome);
    }

    [Fact]
    public void Disabled_rule_never_raises()
    {
        var rule = PressRule();
        rule.Disable();

        var result = AlarmEvaluator.Evaluate(rule, 300, T0.AddMinutes(5), breachStartedAt: T0, hasActiveAlarm: false);

        Assert.Equal(EvaluationOutcome.None, result.Outcome);
        Assert.Null(result.BreachStartedAt);
    }
}
