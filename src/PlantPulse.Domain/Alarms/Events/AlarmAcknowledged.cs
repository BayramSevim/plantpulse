using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms.Events
{
    /// <summary>
    /// Olay: alarm onaylandı.
    /// </summary>
    public sealed record AlarmAcknowledged(Guid AlarmId, string AcknowledgedBy, DateTime AcknowledgedAt) : IDomainEvent;
}