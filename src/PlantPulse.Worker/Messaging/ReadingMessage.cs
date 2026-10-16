namespace PlantPulse.Worker.Messaging;

public sealed record ReadingMessage(string DeviceCode, double Value, DateTime Timestamp);
