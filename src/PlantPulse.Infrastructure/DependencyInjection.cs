using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantPulse.Application.Abstractions;
using PlantPulse.Infrastructure.Persistence;
using PlantPulse.Infrastructure.Readings;
using PlantPulse.Infrastructure.Repositories;
using PlantPulse.Infrastructure.State;

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
        services.AddSingleton<IBreachStateStore, InMemoryBreachStateStore>();
        services.AddScoped<IReadingWriter, ReadingWriter>();

        return services;
    }
}