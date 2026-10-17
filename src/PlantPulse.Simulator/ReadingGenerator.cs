namespace PlantPulse.Simulator;

public sealed class ReadingGenerator
{
    private readonly Random _random = new();

    private readonly Dictionary<string, int> _spikeRemaining = new();

    private const double SpikeChance = 0.005;

    private const int SpikeLength = 45;

    public double Next(DeviceProfile device)
    {
        var noise = (_random.NextDouble() * 2 - 1) * device.Noise;

        var remaining = _spikeRemaining.GetValueOrDefault(device.Code);

        if (remaining > 0)
        {
            _spikeRemaining[device.Code] = remaining - 1;
            return Math.Round(device.BaseValue + device.SpikeOffset + noise, 2);
        }

        if (_random.NextDouble() < SpikeChance)
            _spikeRemaining[device.Code] = SpikeLength;

        return Math.Round(device.BaseValue + noise, 2);
    }
}
