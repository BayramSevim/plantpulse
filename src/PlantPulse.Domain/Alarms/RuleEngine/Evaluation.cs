using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Domain.Alarms.RuleEngine;

/// <summary>
/// Değerlendirme sonucu: karar (Outcome) ve bir sonraki okumada geri verilecek aşım başlangıç zamanı (BreachStartedAt).
/// </summary>
public sealed record Evaluation(EvaluationOutcome Outcome, DateTime? BreachStartedAt);