using PlantPulse.Application.Abstractions;

namespace PlantPulse.Application.Alarms;

public sealed class AcknowledgeAlarmHandler
{
    private readonly IAlarmRepository _alarms;
    private readonly IUnitOfWork _unitOfWork;

    public AcknowledgeAlarmHandler(IAlarmRepository alarms, IUnitOfWork unitOfWork)
    {
        _alarms = alarms;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(Guid alarmId, string acknowledgedBy, CancellationToken ct)
    {
        var alarm = await _alarms.GetByIdAsync(alarmId, ct);
        if (alarm is null)
            return false;

        alarm.Acknowledge(acknowledgedBy, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
