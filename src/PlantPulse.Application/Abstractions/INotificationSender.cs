namespace PlantPulse.Application.Abstractions;

public interface INotificationSender
{
    Task SendAsync(string subject, string body, CancellationToken ct);
}
