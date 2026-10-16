namespace PlantPulse.Simulator;

/// <summary>
/// Simüle edilen bir cihazın davranış profili.
/// </summary>
/// <param name="Code">Cihaz kodu (Kafka mesaj anahtarı olarak da kullanılır).</param>
/// <param name="Type">Ölçülen büyüklük: Temperature, MotorCurrent, SiloLevel.</param>
/// <param name="BaseValue">Normal çalışma değeri; değer bunun etrafında dalgalanır.</param>
/// <param name="Noise">Her okumadaki rastgele oynama miktarı (artı/eksi).</param>
/// <param name="SpikeOffset">Arıza anında değere eklenen sapma (silo için negatif: seviye düşer).</param>
public sealed record DeviceProfile(
    string Code,
    string Type,
    double BaseValue,
    double Noise,
    double SpikeOffset);
