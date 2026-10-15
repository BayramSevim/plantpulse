using PlantPulse.Domain.Common;
using PlantPulse.Domain.Alarms.Events;

namespace PlantPulse.Domain.Alarms
{
    /// <summary>
    /// Alarm kaydı: bir kuralın tetiklendiği tek bir olay. Açılır (Active), kapanır (Cleared), operatör tarafından onaylanır (Acknowledge).
    /// </summary>
    public class Alarm : AggregateRoot
    {
        /// <summary>
        /// Alarmın kimliği.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Alarmı tetikleyen kural.
        /// </summary>
        public Guid RuleId { get; private set; }
        /// <summary>
        /// Alarmın ait olduğu cihaz (kuraldan alınır).
        /// </summary>
        public Guid DeviceId { get; private set; }
        /// <summary>
        /// Önem derecesi (kuraldan alınır).
        /// </summary>
        public AlarmSeverity Severity { get; private set; }
        /// <summary>
        /// Durum: Active (açık) / Cleared (kapandı).
        /// </summary>
        public AlarmStatus Status { get; private set; }

        /// <summary>
        /// Alarmı tetikleyen değer.
        /// </summary>
        public double TriggerValue { get; private set; }
        /// <summary>
        /// Alarmın açıldığı zaman.
        /// </summary>
        public DateTime OpenedAt { get; private set; }

        /// <summary>
        /// Alarm kapandığı andaki değer (açıkken null).
        /// </summary>
        public double? ClearedValue { get; private set; }
        /// <summary>
        /// Alarmın kapandığı zaman (açıkken null).
        /// </summary>
        public DateTime? ClearedAt { get; private set; }

        /// <summary>
        /// Alarmı onaylayan kişi (onaylanmadıysa null).
        /// </summary>
        public string? AcknowledgedBy { get; private set; }
        /// <summary>
        /// Onay zamanı (onaylanmadıysa null).
        /// </summary>
        public DateTime? AcknowledgedAt { get; private set; }

        private Alarm() { }

        /// <summary>
        /// Kuraldan yeni alarm açar ve AlarmRaised (alarm açıldı) olayını üretir.
        /// </summary>
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
            Raise(new AlarmRaised(Id, DeviceId, RuleId, Severity, TriggerValue, OpenedAt));
        }


        /// <summary>
        /// Alarmı kapatır (değer düzeldi) ve AlarmCleared (alarm kapandı) olayını üretir.
        /// </summary>
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
            Raise(new AlarmCleared(Id, DeviceId, ClearedValue.Value, ClearedAt.Value));
        }

        /// <summary>
        /// Alarmı onaylar (operatör "gördüm" der) ve AlarmAcknowledged olayını üretir. Kapanmış alarm da onaylanabilir: kimsenin görmediği bir alarmın izi kaybolmamalı.
        /// </summary>
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
            Raise(new AlarmAcknowledged(Id, AcknowledgedBy!, AcknowledgedAt.Value));
        }
    }
}