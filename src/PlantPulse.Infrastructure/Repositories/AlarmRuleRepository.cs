using Microsoft.EntityFrameworkCore;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Infrastructure.Repositories
{
    public class AlarmRuleRepository : IAlarmRuleRepository
    {
        private readonly PlantPulseDbContext _db;

        public AlarmRuleRepository(PlantPulseDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<AlarmRule>> GetEnabledByDeviceCodeAsync(string deviceCode, CancellationToken ct)
        {
            var query = from r in _db.AlarmRules
                        join d in _db.Devices on r.DeviceId equals d.Id
                        where d.Code == deviceCode && r.IsEnabled && d.IsActive
                        select r;

            return await query.AsNoTracking().ToListAsync(ct);
        }
    }
}