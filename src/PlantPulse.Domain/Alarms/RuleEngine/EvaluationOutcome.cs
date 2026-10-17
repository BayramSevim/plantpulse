using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine
{
    public enum EvaluationOutcome
    {
        None,
        OpenAlarm,
        ClearAlarm
    }
}