using PlantPulse.Domain.Alarms;

namespace PlantPulse.Application.Abstractions
{
    public interface IAlarmRepository
    {
        Task<Alarm?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Alarm?> GetActiveByRuleIdAsync(Guid ruleId, CancellationToken ct);
        Task AddAsync(Alarm alarm, CancellationToken ct);
    }
}
