using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using PlantPulse.Infrastructure.Messaging;

namespace PlantPulse.Api.Live;

public sealed class LiveFeedConsumer : BackgroundService
{
    private readonly IHubContext<PlantHub> _hub;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LiveFeedConsumer> _logger;

    public LiveFeedConsumer(IHubContext<PlantHub> hub, IConfiguration configuration, ILogger<LiveFeedConsumer> logger)
    {
        _hub = hub;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var readingsTopic = _configuration["Kafka:Topic"]!;
        var alarmTopic = _configuration["Kafka:AlarmEventsTopic"]!;

        var config = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:LiveGroupId"],
            AutoOffsetReset = AutoOffsetReset.Latest,
            EnableAutoCommit = true,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(new[] { readingsTopic, alarmTopic });

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> result;
                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning("Live feed consume error: {Reason}", ex.Error.Reason);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                try
                {
                    if (result.Topic == readingsTopic)
                    {
                        var reading = JsonSerializer.Deserialize<LiveReading>(result.Message.Value);
                        if (reading is not null)
                            await _hub.Clients.All.SendAsync("reading", reading, stoppingToken);
                    }
                    else
                    {
                        var envelope = JsonSerializer.Deserialize<AlarmEventEnvelope>(result.Message.Value);
                        if (envelope is not null)
                        {
                            var alarmId = envelope.Payload.GetProperty("AlarmId").GetGuid();
                            await _hub.Clients.All.SendAsync("alarm", new LiveAlarmEvent(envelope.Type, alarmId), stoppingToken);
                        }
                    }
                }
                catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
                {
                    _logger.LogWarning(ex, "Skipped invalid live message on {Topic}", result.Topic);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            consumer.Close();
        }
    }

    private sealed record LiveReading(string DeviceCode, double Value, DateTime Timestamp);

    private sealed record LiveAlarmEvent(string Type, Guid AlarmId);
}
