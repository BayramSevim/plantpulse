using PlantPulse.Application.Readings;

namespace PlantPulse.Application.Abstractions;

public interface ILatestValueStore
{
    Task SetManyAsync(IReadOnlyCollection<ReadingData> readings, CancellationToken ct);

    Task<IReadOnlyList<ReadingData>> GetAllAsync(CancellationToken ct);
}
