using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine
{
    /// <summary>
    /// Alarm değerlendirici (kural motoru): gelen değere bakıp alarm açılsın mı, kapansın mı, yoksa bir şey yapılmasın mı karar verir. Saf fonksiyon: veritabanı veya saat kullanmaz, her şeyi parametre olarak alır.
    /// </summary>
    public static class AlarmEvaluator
    {
        /// <summary>
        /// Değerlendir: kurala, gelen değere, aşımın başladığı zamana ve açık alarm olup olmadığına bakıp kararı döndürür.
        /// </summary>
        public static Evaluation Evaluate(
            AlarmRule rule,
            double value,
            DateTime at,
            DateTime? breachStartedAt, // Aşımın Başladığı Zaman
            bool hasActiveAlarm // Açıkta Bir Alarm Var Mı?
        )
        {
            if (!rule.IsEnabled)
                return new Evaluation(EvaluationOutcome.None, null);

            if (hasActiveAlarm)
            {
                if (rule.Threshold.IsRecoveredBy(value))
                    return new Evaluation(EvaluationOutcome.ClearAlarm, null);
                return new Evaluation(EvaluationOutcome.None, null);
            }

            if (!rule.Threshold.IsBreachedBy(value))
                return new Evaluation(EvaluationOutcome.None, null);

            var start = breachStartedAt ?? at;
            if (at - start >= rule.Duration)
                return new Evaluation(EvaluationOutcome.OpenAlarm, start);

            return new Evaluation(EvaluationOutcome.None, start);
        }
    }
}