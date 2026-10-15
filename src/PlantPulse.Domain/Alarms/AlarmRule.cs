using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms
{
    /// <summary>
    /// Alarm kuralı: hangi cihazda, hangi eşik, ne kadar süre aşılırsa, hangi önemde alarm açılacağı.
    /// </summary>
    public class AlarmRule
    {
        /// <summary>
        /// Kuralın kimliği.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Kuralın ait olduğu cihaz.
        /// </summary>
        public Guid DeviceId { get; private set; }
        /// <summary>
        /// Eşik (sınır, yön, histerezis).
        /// </summary>
        public Threshold Threshold { get; private set; }
        /// <summary>
        /// Süre: eşik kesintisiz bu kadar aşılırsa alarm açılır.
        /// </summary>
        public TimeSpan Duration { get; private set; }
        /// <summary>
        /// Önem derecesi: Warning (uyarı) / Critical (kritik).
        /// </summary>
        public AlarmSeverity Severity { get; private set; }
        /// <summary>
        /// Kural aktif mi? Bakımdaki makine sahte alarm üretmesin diye kapatılabilir.
        /// </summary>
        public bool IsEnabled { get; private set; }

        private AlarmRule()
        {

        }

        /// <summary>
        /// Yeni kural oluşturur. Cihaz ve eşik zorunlu, süre sıfırdan büyük olmalı.
        /// </summary>
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

        /// <summary>
        /// Kuralı aktif eder.
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
        }

        /// <summary>
        /// Kuralı devre dışı bırakır (örn. bakım sırasında).
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
        }
    }
}