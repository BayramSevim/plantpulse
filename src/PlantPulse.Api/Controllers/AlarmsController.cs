using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantPulse.Api.Contracts;
using PlantPulse.Application.Alarms;
using PlantPulse.Domain.Alarms;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Api.Controllers;

[ApiController]
[Route("api/alarms")]
public sealed class AlarmsController : ControllerBase
{
    private readonly PlantPulseDbContext _db;

    public AlarmsController(PlantPulseDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IReadOnlyList<AlarmDto>> Get([FromQuery] string? status, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 200);

        var query = _db.Alarms.AsNoTracking();

        if (string.Equals(status, "active", StringComparison.OrdinalIgnoreCase))
            query = query.Where(a => a.Status == AlarmStatus.Active);
        else if (string.Equals(status, "cleared", StringComparison.OrdinalIgnoreCase))
            query = query.Where(a => a.Status == AlarmStatus.Cleared);

        var alarms = await query
            .OrderByDescending(a => a.OpenedAt)
            .Take(limit)
            .ToListAsync(ct);

        var deviceIds = alarms.Select(a => a.DeviceId).Distinct().ToList();
        var devices = await _db.Devices.AsNoTracking()
            .Where(d => deviceIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, ct);

        return alarms.Select(a =>
        {
            devices.TryGetValue(a.DeviceId, out var device);
            return new AlarmDto(
                a.Id,
                a.DeviceId,
                device?.Code ?? "?",
                device?.Name ?? "?",
                a.Severity.ToString(),
                a.Status.ToString(),
                a.TriggerValue,
                a.OpenedAt,
                a.ClearedValue,
                a.ClearedAt,
                a.AcknowledgedBy,
                a.AcknowledgedAt);
        }).ToList();
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(
        Guid id,
        [FromBody] AcknowledgeRequest request,
        [FromServices] AcknowledgeAlarmHandler handler,
        CancellationToken ct)
    {
        var found = await handler.HandleAsync(id, request.By, ct);
        return found ? NoContent() : NotFound();
    }
}
