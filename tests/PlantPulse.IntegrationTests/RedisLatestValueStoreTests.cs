using PlantPulse.Application.Readings;
using PlantPulse.Infrastructure.Caching;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace PlantPulse.IntegrationTests;

public sealed class RedisLatestValueStoreTests : IAsyncLifetime
{
    private static readonly DateTime T0 = new(2026, 10, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly RedisContainer _container = new RedisBuilder().WithImage("redis:7").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task Keeps_only_the_newest_reading_per_device()
    {
        using var redis = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
        var store = new RedisLatestValueStore(redis);

        await store.SetManyAsync(new[]
        {
            new ReadingData("PRESS-02", 225, T0.AddSeconds(1)),
            new ReadingData("PRESS-02", 190, T0),
            new ReadingData("SILO-C", 30, T0)
        }, CancellationToken.None);

        var all = await store.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, all.Count);
        var press = Assert.Single(all, r => r.DeviceCode == "PRESS-02");
        Assert.Equal(225, press.Value);
        Assert.Equal(T0.AddSeconds(1), press.Timestamp);
    }
}
