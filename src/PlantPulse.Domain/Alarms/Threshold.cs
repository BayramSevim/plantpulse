using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms;


/// <summary>
/// Eşik (value object): sınır, yön ve histerezis. Değişmezdir, değerlerine göre karşılaştırılır.
/// </summary>
public sealed record Threshold
{
    /// <summary>
    /// Sınır değeri.
    /// </summary>
    public double Limit { get; }
    /// <summary>
    /// Yön: Above (üstü) / Below (altı).
    /// </summary>
    public ThresholdDirection Direction { get; }
    /// <summary>
    /// Histerezis (kapanma payı): alarmın kapanması için değerin sınırdan ne kadar uzaklaşması gerektiği. Örn. sınır 95, histerezis 3 ise alarm 92'de kapanır.
    /// </summary>
    public double Hysteresis { get; }

    /// <summary>
    /// Yeni eşik oluşturur. Sınır sonlu bir sayı olmalı, histerezis negatif olamaz.
    /// </summary>
    public Threshold(double limit, ThresholdDirection direction, double hysteresis)
    {
        if (!double.IsFinite(limit))
            throw new DomainException("Limit must be a finite number.");
        if (!double.IsFinite(hysteresis) || hysteresis < 0)
            throw new DomainException("Hysteresis cannot be negative.");

        Limit = limit;
        Direction = direction;
        Hysteresis = hysteresis;
    }

    /// <summary>
    /// Bu değer sınırı aştı mı? (alarm açılma koşulu)
    /// </summary>
    public bool IsBreachedBy(double value) => Direction switch
    {
        ThresholdDirection.Above => value > Limit,
        ThresholdDirection.Below => value < Limit,
        _ => throw new DomainException($"Unknown direction: {Direction}")
    };

    /// <summary>
    /// Bu değerle durum düzeldi mi? Histerezis hesaba katılır. (alarm kapanma koşulu)
    /// </summary>
    public bool IsRecoveredBy(double value) => Direction switch
    {
        ThresholdDirection.Above => value <= Limit - Hysteresis,
        ThresholdDirection.Below => value >= Limit + Hysteresis,
        _ => throw new DomainException($"Unknown direction: {Direction}")
    };
}
