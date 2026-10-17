namespace PlantPulse.Api.Contracts;

public sealed record DeviceSummaryDto(
    Guid Id,
    string Code,
    string Name,
    string Type,
    string Unit,
    double? Value,
    DateTime? Timestamp,
    double? ThresholdLimit,
    string? ThresholdDirection,
    double? Hysteresis,
    int? DurationSeconds,
    string? ActiveAlarmSeverity);
