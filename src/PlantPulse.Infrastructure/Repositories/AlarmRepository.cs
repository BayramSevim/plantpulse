using Microsoft.EntityFrameworkCore;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Infrastructure.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly PlantPulseDbContext _db;

        public AlarmRepository(PlantPulseDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Alarm alarm, CancellationToken ct)
        {
            await _db.Alarms.AddAsync(alarm, ct);
        }

        public Task<Alarm?> GetActiveByRuleIdAsync(Guid ruleId, CancellationToken ct)
        {
            return _db.Alarms.FirstOrDefaultAsync(a => a.RuleId == ruleId && a.Status == AlarmStatus.Active, ct);
        }
    }
}