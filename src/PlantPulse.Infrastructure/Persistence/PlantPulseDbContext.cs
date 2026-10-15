using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;

namespace PlantPulse.Infrastructure.Persistence
{
    public class PlantPulseDbContext : DbContext
    {

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<AlarmRule> AlarmRules => Set<AlarmRule>();
        public DbSet<Alarm> Alarms => Set<Alarm>();

        public PlantPulseDbContext(DbContextOptions<PlantPulseDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlantPulseDbContext).Assembly);
        }
    }
}