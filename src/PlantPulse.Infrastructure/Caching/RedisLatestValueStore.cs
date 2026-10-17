using System.Text.Json;
using PlantPulse.Application.Abstractions;
using PlantPulse.Application.Readings;
using StackExchange.Redis;

namespace PlantPulse.Infrastructure.Caching;

public sealed class RedisLatestValueStore : ILatestValueStore
{
    private const string Key = "plantpulse:latest";

    private readonly IConnectionMultiplexer _redis;

    public RedisLatestValueStore(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetManyAsync(IReadOnlyCollection<ReadingData> readings, CancellationToken ct)
    {
        if (readings.Count == 0)
            return;

        var entries = readings
            .GroupBy(r => r.DeviceCode)
            .Select(g => g.MaxBy(r => r.Timestamp)!)
            .Select(r => new HashEntry(r.DeviceCode, JsonSerializer.Serialize(new CachedValue(r.Value, r.Timestamp))))
            .ToArray();

        await _redis.GetDatabase().HashSetAsync(Key, entries);
    }

    public async Task<IReadOnlyList<ReadingData>> GetAllAsync(CancellationToken ct)
    {
        var entries = await _redis.GetDatabase().HashGetAllAsync(Key);

        var result = new List<ReadingData>(entries.Length);
        foreach (var entry in entries)
        {
            var cached = JsonSerializer.Deserialize<CachedValue>(entry.Value.ToString());
            if (cached is not null)
                result.Add(new ReadingData(entry.Name.ToString(), cached.Value, cached.Timestamp));
        }

        return result;
    }

    private sealed record CachedValue(double Value, DateTime Timestamp);
}
