namespace PlantPulse.Simulator;

public sealed record DeviceProfile(
    string Code,
    string Type,
    double BaseValue,
    double Noise,
    double SpikeOffset);
