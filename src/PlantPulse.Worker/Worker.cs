using System.Text.Json;
using Confluent.Kafka;
using PlantPulse.Application.Readings;
using PlantPulse.Worker.Messaging;

namespace PlantPulse.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        var topic = _configuration["Kafka:Topic"];

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false // mesajı işledikten SONRA kendimiz commit edeceğiz
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                try
                {
                    var message = JsonSerializer.Deserialize<ReadingMessage>(result.Message.Value);

                    _logger.LogInformation("Received {DeviceCode} = {Value} (partition {Partition})",
                        message?.DeviceCode, message?.Value, result.Partition.Value);

                    if (message is not null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<ReadingProcessor>();
                        await processor.ProcessAsync(message.DeviceCode, message.Value, message.Timestamp, stoppingToken);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Invalid message skipped at offset {Offset}", result.Offset.Value);
                }

                consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker stopping");
        }
        finally
        {
            consumer.Close();
        }
    }
}
