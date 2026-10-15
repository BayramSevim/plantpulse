using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms
{
    public class Alarm
    {
        public Guid Id { get; private set; }
        public Guid RuleId { get; private set; }
        public Guid DeviceId { get; private set; }
        public AlarmSeverity Severity { get; private set; }
        public AlarmStatus Status { get; private set; }

        public double TriggerValue { get; private set; }
        public DateTime OpenedAt { get; private set; }

        public double? ClearedValue { get; private set; }
        public DateTime? ClearedAt { get; private set; }

        public string? AcknowledgedBy { get; private set; }
        public DateTime? AcknowledgedAt { get; private set; }

        private Alarm() { }

        public Alarm(AlarmRule rule, double triggerValue, DateTime openedAt)
        {
            if (rule is null)
                throw new DomainException("Rule is required");
            if (!double.IsFinite(triggerValue))
                throw new DomainException("Trigger value must be a finite number");

            Id = Guid.NewGuid();
            RuleId = rule.Id;
            DeviceId = rule.DeviceId;
            Severity = rule.Severity;
            TriggerValue = triggerValue;
            OpenedAt = openedAt;
            Status = AlarmStatus.Active;
        }

        public void Clear(double value, DateTime at)
        {
            if (Status == AlarmStatus.Cleared)
                throw new DomainException("Alarm is already cleared");
            if (!double.IsFinite(value))
                throw new DomainException("Cleared value must be a finite number");
            if (at < OpenedAt)
                throw new DomainException("Alarm cannot be cleared before it was opened");

            Status = AlarmStatus.Cleared;
            ClearedValue = value;
            ClearedAt = at;
        }

        public void Acknowledge(string by, DateTime at)
        {
            if (string.IsNullOrWhiteSpace(by))
                throw new DomainException("Acknowledging user is required");
            if (AcknowledgedAt is not null)
                throw new DomainException("Alarm is already acknowledged");
            if (at < OpenedAt)
                throw new DomainException("Alarm cannot be acknowledged before it was opened");

            AcknowledgedBy = by.Trim();
            AcknowledgedAt = at;
        }
    }
}