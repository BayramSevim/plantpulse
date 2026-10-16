namespace PlantPulse.Simulator;

/// <summary>
/// Cihazlar için gerçekçi okuma değerleri üretir.
/// Normalde değer BaseValue etrafında küçük dalgalanır; arada bir "arıza" başlar
/// ve değer bir süre SpikeOffset kadar kayar (alarm tetiklensin diye).
/// </summary>
public sealed class ReadingGenerator
{
    private readonly Random _random = new();

    // Her cihazın arızası kaç okuma daha sürecek? (0 = arıza yok)
    private readonly Dictionary<string, int> _spikeRemaining = new();

    /// <summary>
    /// Arızanın başlama ihtimali (her okumada). 0.005 = binde 5.
    /// 10 cihaz, saniyede 1 okuma: ortalama ~20 saniyede bir cihazda arıza başlar.
    /// </summary>
    private const double SpikeChance = 0.005;

    /// <summary>
    /// Bir arıza kaç okuma (saniye) sürer. Kural süresinden (30 sn) uzun olmalı ki alarm açılabilsin.
    /// </summary>
    private const int SpikeLength = 45;

    /// <summary>
    /// Cihaz için bir sonraki değeri üretir.
    /// </summary>
    public double Next(DeviceProfile device)
    {
        // 1) Gürültü: sensör değeri hiçbir zaman sabit durmaz, -Noise ile +Noise arasında titrer.
        var noise = (_random.NextDouble() * 2 - 1) * device.Noise;

        // 2) Bu cihaz şu an arızada mı? (kayıt yoksa 0 = arıza yok)
        var remaining = _spikeRemaining.GetValueOrDefault(device.Code);

        // 3) Arızadaysa: değeri SpikeOffset kadar kaydır, sayacı bir azalt.
        if (remaining > 0)
        {
            _spikeRemaining[device.Code] = remaining - 1;
            return Math.Round(device.BaseValue + device.SpikeOffset + noise, 2);
        }

        // 4) Arıza yoksa: küçük bir ihtimalle yeni arıza başlat (bir sonraki okumadan itibaren etkili).
        if (_random.NextDouble() < SpikeChance)
            _spikeRemaining[device.Code] = SpikeLength;

        // 5) Normal değer: BaseValue etrafında titreme.
        return Math.Round(device.BaseValue + noise, 2);
    }
}
