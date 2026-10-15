using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms.Events
{
    /// <summary>
    /// Olay: alarm kapandı.
    /// </summary>
    public sealed record AlarmCleared(Guid AlarmId, Guid DeviceId, double ClearedValue, DateTime ClearedAt) : IDomainEvent;
}