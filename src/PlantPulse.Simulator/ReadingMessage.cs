namespace PlantPulse.Simulator;

/// <summary>
/// Kafka'ya gönderilen tek bir sensör okuması. Worker bu formatı okuyacak;
/// iki proje birbirini tanımıyor, sadece bu JSON şekli üzerinde anlaşıyor.
/// </summary>
public sealed record ReadingMessage(string DeviceCode, double Value, DateTime Timestamp);
