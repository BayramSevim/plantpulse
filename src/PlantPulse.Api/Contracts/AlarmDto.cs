namespace PlantPulse.Api.Contracts;

public sealed record AlarmDto(
    Guid Id,
    Guid DeviceId,
    string DeviceCode,
    string DeviceName,
    string Severity,
    string Status,
    double TriggerValue,
    DateTime OpenedAt,
    double? ClearedValue,
    DateTime? ClearedAt,
    string? AcknowledgedBy,
    DateTime? AcknowledgedAt);

public sealed record AcknowledgeRequest(string By);
