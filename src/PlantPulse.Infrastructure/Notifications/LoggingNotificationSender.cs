using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlantPulse.Application.Abstractions;

namespace PlantPulse.Infrastructure.Notifications;

public sealed class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;
    private readonly double _failureRate;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger, IConfiguration configuration)
    {
        _logger = logger;

        _failureRate = double.TryParse(configuration["Notifications:FailureRate"], NumberStyles.Float,
            CultureInfo.InvariantCulture, out var rate)
            ? Math.Clamp(rate, 0, 1)
            : 0;
    }

    public Task SendAsync(string subject, string body, CancellationToken ct)
    {
        if (Random.Shared.NextDouble() < _failureRate)
            throw new InvalidOperationException("Mail server temporarily unavailable (simulated).");

        _logger.LogInformation("EMAIL sent: {Subject} | {Body}", subject, body);
        return Task.CompletedTask;
    }
}
