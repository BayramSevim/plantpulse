using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine;

public sealed record Evaluation(EvaluationOutcome Outcome, DateTime? BreachStartedAt);