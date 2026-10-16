using PlantPulse.Domain.Alarms;

namespace PlantPulse.Application.Abstractions
{
    public interface IAlarmRuleRepository
    {
        Task<IReadOnlyList<AlarmRule>> GetEnabledByDeviceCodeAsync(string deviceCode, CancellationToken ct);
    }
}