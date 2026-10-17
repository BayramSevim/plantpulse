using Microsoft.EntityFrameworkCore;
using PlantPulse.Application.Readings;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;
using PlantPulse.Infrastructure.Persistence;
using PlantPulse.Infrastructure.Repositories;
using PlantPulse.Infrastructure.State;

namespace PlantPulse.IntegrationTests;

public sealed class ReadingProcessorTests : IClassFixture<PostgresFixture>
{
    private static readonly DateTime T0 = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly PostgresFixture _db;

    public ReadingProcessorTests(PostgresFixture db)
    {
        _db = db;
    }

    [Fact]
    public async Task Opens_alarm_after_duration_and_clears_after_recovery_with_outbox_events()
    {
        var rule = await SeedDeviceWithRuleAsync("PRESS-T1");
        var breachStore = new InMemoryBreachStateStore();

        await ProcessAsync(breachStore, "PRESS-T1", 225, T0);
        await ProcessAsync(breachStore, "PRESS-T1", 226, T0.AddSeconds(15));
        await using (var db = _db.CreateDbContext())
        {
            Assert.Empty(await db.Alarms.Where(a => a.RuleId == rule.Id).ToListAsync());
        }

        await ProcessAsync(breachStore, "PRESS-T1", 227, T0.AddSeconds(30));
        Guid alarmId;
        await using (var db = _db.CreateDbContext())
        {
            var alarm = Assert.Single(await db.Alarms.Where(a => a.RuleId == rule.Id).ToListAsync());
            Assert.Equal(AlarmStatus.Active, alarm.Status);
            Assert.Equal(227, alarm.TriggerValue);
            alarmId = alarm.Id;

            Assert.Contains(await db.OutboxMessages.ToListAsync(),
                m => m.Type == "AlarmRaised" && m.Payload.Contains(alarmId.ToString()));
        }

        await ProcessAsync(breachStore, "PRESS-T1", 207, T0.AddSeconds(40));
        await using (var db = _db.CreateDbContext())
        {
            Assert.Equal(AlarmStatus.Active, (await db.Alarms.SingleAsync(a => a.Id == alarmId)).Status);
        }

        await ProcessAsync(breachStore, "PRESS-T1", 204, T0.AddSeconds(50));
        await using (var db = _db.CreateDbContext())
        {
            var alarm = await db.Alarms.SingleAsync(a => a.Id == alarmId);
            Assert.Equal(AlarmStatus.Cleared, alarm.Status);
            Assert.Equal(204, alarm.ClearedValue);

            Assert.Contains(await db.OutboxMessages.ToListAsync(),
                m => m.Type == "AlarmCleared" && m.Payload.Contains(alarmId.ToString()));
        }
    }

    [Fact]
    public async Task Database_rejects_a_second_active_alarm_for_the_same_rule()
    {
        var rule = await SeedDeviceWithRuleAsync("PRESS-T2");

        await using var db = _db.CreateDbContext();
        db.Alarms.Add(new Alarm(rule, 225, T0));
        db.Alarms.Add(new Alarm(rule, 226, T0.AddSeconds(1)));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    private async Task<AlarmRule> SeedDeviceWithRuleAsync(string code)
    {
        await using var db = _db.CreateDbContext();

        var device = new Device(code, "Test Presi", DeviceType.MotorCurrent);
        var rule = new AlarmRule(device.Id, new Threshold(210, ThresholdDirection.Above, 5),
            TimeSpan.FromSeconds(30), AlarmSeverity.Critical);

        db.Devices.Add(device);
        db.AlarmRules.Add(rule);
        await db.SaveChangesAsync();

        return rule;
    }

    private async Task ProcessAsync(InMemoryBreachStateStore breachStore, string code, double value, DateTime at)
    {
        await using var db = _db.CreateDbContext();
        var processor = new ReadingProcessor(new AlarmRuleRepository(db), new AlarmRepository(db), breachStore, db);
        await processor.ProcessAsync(code, value, at, CancellationToken.None);
    }
}
