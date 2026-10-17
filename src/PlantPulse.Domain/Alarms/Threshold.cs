using PlantPulse.Domain.Common;

namespace PlantPulse.Domain.Alarms;

public sealed record Threshold
{
    public double Limit { get; }
    public ThresholdDirection Direction { get; }
    public double Hysteresis { get; }

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

    public bool IsBreachedBy(double value) => Direction switch
    {
        ThresholdDirection.Above => value > Limit,
        ThresholdDirection.Below => value < Limit,
        _ => throw new DomainException($"Unknown direction: {Direction}")
    };

    public bool IsRecoveredBy(double value) => Direction switch
    {
        ThresholdDirection.Above => value <= Limit - Hysteresis,
        ThresholdDirection.Below => value >= Limit + Hysteresis,
        _ => throw new DomainException($"Unknown direction: {Direction}")
    };
}
