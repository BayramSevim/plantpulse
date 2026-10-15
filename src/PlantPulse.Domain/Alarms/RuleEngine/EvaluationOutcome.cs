using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine
{
    /// <summary>
    /// Değerlendirme sonucunun türü.
    /// </summary>
    public enum EvaluationOutcome
    {
        /// <summary>
        /// Bir şey yapma.
        /// </summary>
        None,
        /// <summary>
        /// Alarm aç.
        /// </summary>
        OpenAlarm,
        /// <summary>
        /// Alarmı kapat.
        /// </summary>
        ClearAlarm
    }
}