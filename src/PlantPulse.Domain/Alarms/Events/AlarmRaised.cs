using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms.Events
{
    /// <summary>
    /// Olay: alarm açıldı.
    /// </summary>
    public sealed record AlarmRaised(
    Guid AlarmId,
    Guid DeviceId,
    Guid RuleId,
    AlarmSeverity Severity,
    double TriggerValue,
    DateTime OpenedAt) : IDomainEvent;
}