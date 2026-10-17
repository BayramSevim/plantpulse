namespace PlantPulse.Simulator;

public sealed record ReadingMessage(string DeviceCode, double Value, DateTime Timestamp);
