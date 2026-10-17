using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine
{
    public static class AlarmEvaluator
    {
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