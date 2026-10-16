using System.Text.Json;
using Confluent.Kafka;
using PlantPulse.Worker.Messaging;

namespace PlantPulse.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consume() thread'i bloklar; host açılışını bekletmemek için arka plana geç.
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
