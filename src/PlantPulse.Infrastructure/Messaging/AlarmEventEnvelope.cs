using System.Text.Json;

namespace PlantPulse.Infrastructure.Messaging;

public sealed record AlarmEventEnvelope(Guid EventId, string Type, DateTime OccurredAt, JsonElement Payload);
