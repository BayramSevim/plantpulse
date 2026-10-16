using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PlantPulseDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PlantPulse")));

        return services;
    }
}