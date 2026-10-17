using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms.Events;
using PlantPulse.Infrastructure.Messaging;

namespace PlantPulse.Infrastructure.Outbox;

public sealed class OutboxDispatcher
{
    private readonly IProducer<string, string> _producer;
    private readonly INotificationSender _notifications;
    private readonly string _topic;

    public OutboxDispatcher(IProducer<string, string> producer, INotificationSender notifications, IConfiguration configuration)
    {
        _producer = producer;
        _notifications = notifications;
        _topic = configuration["Kafka:AlarmEventsTopic"]
                 ?? throw new InvalidOperationException("Kafka:AlarmEventsTopic is not configured.");
    }

    public async Task DispatchAsync(OutboxMessage message, CancellationToken ct)
    {
        using var payload = JsonDocument.Parse(message.Payload);

        var key = payload.RootElement.GetProperty("AlarmId").GetString()!;

        var envelope = new AlarmEventEnvelope(message.Id, message.Type, message.OccurredAt, payload.RootElement);

        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = key,
            Value = JsonSerializer.Serialize(envelope)
        }, ct);

        if (message.Type is nameof(AlarmRaised) or nameof(AlarmCleared))
        {
            await _notifications.SendAsync($"[PlantPulse] {message.Type}", message.Payload, ct);
        }
    }
}
