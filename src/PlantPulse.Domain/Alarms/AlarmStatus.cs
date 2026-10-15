using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms
{
    /// <summary>
    /// Alarmın durumu. Makinenin söylediği bilgidir; onaydan (insan) bağımsızdır.
    /// </summary>
    public enum AlarmStatus
    {
        /// <summary>
        /// Açık: değer hâlâ sorunlu.
        /// </summary>
        Active,
        /// <summary>
        /// Kapandı: değer düzeldi.
        /// </summary>
        Cleared
    }
}