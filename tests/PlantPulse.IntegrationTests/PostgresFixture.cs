using Microsoft.EntityFrameworkCore;
using PlantPulse.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace PlantPulse.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .Build();

    public PlantPulseDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<PlantPulseDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
