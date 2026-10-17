using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxDispatcher _dispatcher;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, OutboxDispatcher dispatcher, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox batch failed");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlantPulseDbContext>();

        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        var now = DateTime.UtcNow;

        var messages = await db.OutboxMessages
            .FromSql($@"SELECT * FROM outbox_messages
                        WHERE ""ProcessedAt"" IS NULL AND ""NextAttemptAt"" <= {now}
                        ORDER BY ""OccurredAt""
                        LIMIT {BatchSize}
                        FOR UPDATE SKIP LOCKED")
            .ToListAsync(ct);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                await _dispatcher.DispatchAsync(message, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.AttemptCount++;
                message.Error = ex.Message;

                if (message.AttemptCount >= MaxAttempts)
                {
                    message.NextAttemptAt = null;
                    _logger.LogError(ex, "Outbox message {Id} ({Type}) failed {Attempts} times, giving up",
                        message.Id, message.Type, message.AttemptCount);
                }
                else
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, message.AttemptCount));
                    message.NextAttemptAt = DateTime.UtcNow + delay;
                    _logger.LogWarning("Outbox message {Id} ({Type}) failed (attempt {Attempt}/{Max}), retrying in {Delay}s: {Error}",
                        message.Id, message.Type, message.AttemptCount, MaxAttempts, delay.TotalSeconds, ex.Message);
                }
            }
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        _logger.LogInformation("Outbox: {Count} message(s) handled", messages.Count);
    }
}
