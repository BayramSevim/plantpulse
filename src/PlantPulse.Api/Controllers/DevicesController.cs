using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantPulse.Api.Contracts;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Api.Controllers;

[ApiController]
[Route("api/devices")]
public sealed class DevicesController : ControllerBase
{
    private readonly PlantPulseDbContext _db;
    private readonly ILatestValueStore _latest;

    public DevicesController(PlantPulseDbContext db, ILatestValueStore latest)
    {
        _db = db;
        _latest = latest;
    }

    [HttpGet]
    public async Task<IReadOnlyList<DeviceSummaryDto>> GetAll(CancellationToken ct)
    {
        var devices = await _db.Devices.AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Code)
            .ToListAsync(ct);

        var rules = await _db.AlarmRules.AsNoTracking().ToListAsync(ct);

        var activeAlarms = await _db.Alarms.AsNoTracking()
            .Where(a => a.Status == AlarmStatus.Active)
            .Select(a => new { a.DeviceId, a.Severity })
            .ToListAsync(ct);

        var latest = (await _latest.GetAllAsync(ct)).ToDictionary(r => r.DeviceCode);

        return devices.Select(d =>
        {
            var rule = rules.FirstOrDefault(r => r.DeviceId == d.Id);
            latest.TryGetValue(d.Code, out var last);

            AlarmSeverity? severity = activeAlarms
                .Where(a => a.DeviceId == d.Id)
                .Select(a => (AlarmSeverity?)a.Severity)
                .OrderByDescending(s => s)
                .FirstOrDefault();

            return new DeviceSummaryDto(
                d.Id,
                d.Code,
                d.Name,
                d.Type.ToString(),
                UnitOf(d.Type),
                last?.Value,
                last?.Timestamp,
                rule?.Threshold.Limit,
                rule?.Threshold.Direction.ToString(),
                rule?.Threshold.Hysteresis,
                rule is null ? null : (int)rule.Duration.TotalSeconds,
                severity?.ToString());
        }).ToList();
    }

    [HttpGet("{code}/readings")]
    public async Task<IReadOnlyList<ReadingPointDto>> GetReadings(string code, [FromQuery] int minutes = 5, CancellationToken ct = default)
    {
        minutes = Math.Clamp(minutes, 1, 60);
        var since = DateTime.UtcNow.AddMinutes(-minutes);
        var deviceCode = code.Trim().ToUpperInvariant();

        return await _db.Readings.AsNoTracking()
            .Where(r => r.DeviceCode == deviceCode && r.Timestamp >= since)
            .OrderBy(r => r.Timestamp)
            .Select(r => new ReadingPointDto(r.Timestamp, r.Value))
            .ToListAsync(ct);
    }

    private static string UnitOf(DeviceType type) => type switch
    {
        DeviceType.Temperature => "°C",
        DeviceType.MotorCurrent => "A",
        DeviceType.SiloLevel => "%",
        _ => ""
    };
}
