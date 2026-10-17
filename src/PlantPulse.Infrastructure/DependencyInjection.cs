using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantPulse.Application.Abstractions;
using PlantPulse.Infrastructure.Caching;
using PlantPulse.Infrastructure.Notifications;
using PlantPulse.Infrastructure.Outbox;
using PlantPulse.Infrastructure.Persistence;
using PlantPulse.Infrastructure.Readings;
using PlantPulse.Infrastructure.Repositories;
using PlantPulse.Infrastructure.State;
using StackExchange.Redis;

namespace PlantPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PlantPulseDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PlantPulse")));

        services.AddScoped<IAlarmRuleRepository, AlarmRuleRepository>();
        services.AddScoped<IAlarmRepository, AlarmRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PlantPulseDbContext>());
        services.AddScoped<IReadingWriter, ReadingWriter>();

        services.AddSingleton<IBreachStateStore, InMemoryBreachStateStore>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(
                configuration.GetConnectionString("Redis")
                ?? throw new InvalidOperationException("ConnectionStrings:Redis is not configured."));

            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ILatestValueStore, RedisLatestValueStore>();

        services.AddSingleton<IProducer<string, string>>(_ =>
            new ProducerBuilder<string, string>(new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true
            }).Build());

        services.AddSingleton<INotificationSender, LoggingNotificationSender>();
        services.AddSingleton<OutboxDispatcher>();

        return services;
    }
}
