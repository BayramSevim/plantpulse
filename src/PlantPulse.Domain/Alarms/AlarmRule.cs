using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms
{
    public class AlarmRule
    {
        public Guid Id { get; private set; }
        public Guid DeviceId { get; private set; }
        public Threshold Threshold { get; private set; }
        public TimeSpan Duration { get; private set; }
        public AlarmSeverity Severity { get; private set; }
        public bool IsEnabled { get; private set; }

        private AlarmRule()
        {
        }

        public AlarmRule(Guid deviceId, Threshold threshold, TimeSpan duration, AlarmSeverity alarmSeverity)
        {
            if (duration <= TimeSpan.Zero)
                throw new DomainException("Duration must be greater than zero");
            if (deviceId == Guid.Empty)
                throw new DomainException("DeviceId is required");

            Id = Guid.NewGuid();
            DeviceId = deviceId;
            Threshold = threshold ?? throw new DomainException("Threshold is required");
            Duration = duration;
            Severity = alarmSeverity;
            IsEnabled = true;
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }
    }
}