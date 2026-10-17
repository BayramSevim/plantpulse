using System.Text.Json;
using Confluent.Kafka;
using PlantPulse.Application.Abstractions;
using PlantPulse.Application.Readings;
using PlantPulse.Worker.Messaging;

namespace PlantPulse.Worker;

public class Worker : BackgroundService
{
    private const int MaxBatchSize = 200;
    private static readonly TimeSpan MaxBatchWait = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan PollTimeout = TimeSpan.FromMilliseconds(100);

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
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var batch = new List<ReadingMessage>(MaxBatchSize);
                var hasAny = false;
                var deadline = DateTime.UtcNow + MaxBatchWait;

                while (batch.Count < MaxBatchSize
                        && DateTime.UtcNow < deadline
                        && !stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(PollTimeout);
                    if (result is null)
                        continue;

                    hasAny = true;

                    try
                    {
                        var message = JsonSerializer.Deserialize<ReadingMessage>(result.Message.Value);
                        if (message is not null)
                            batch.Add(message);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Invalid message skipped at {Partition}/{Offset}",
                            result.Partition.Value, result.Offset.Value);
                    }
                }

                if (!hasAny)
                    continue;

                if (batch.Count > 0)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var writer = scope.ServiceProvider.GetRequiredService<IReadingWriter>();
                    var processor = scope.ServiceProvider.GetRequiredService<ReadingProcessor>();

                    var readings = batch
                        .Select(m => new ReadingData(m.DeviceCode, m.Value, m.Timestamp))
                        .ToList();
                    await writer.WriteAsync(readings, stoppingToken);

                    var latestStore = scope.ServiceProvider.GetRequiredService<ILatestValueStore>();
                    try
                    {
                        await latestStore.SetManyAsync(readings, stoppingToken);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogWarning(ex, "Could not update latest values in Redis");
                    }

                    foreach (var m in batch)
                        await processor.ProcessAsync(m.DeviceCode, m.Value, m.Timestamp, stoppingToken);

                    _logger.LogInformation("Processed batch of {Count} readings", batch.Count);
                }

                consumer.Commit();
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
