using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlantPulse.Application.Abstractions;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Common;
using PlantPulse.Domain.Devices;
using PlantPulse.Infrastructure.Outbox;
using PlantPulse.Infrastructure.Readings;

namespace PlantPulse.Infrastructure.Persistence
{
    public class PlantPulseDbContext : DbContext, IUnitOfWork
    {

        public DbSet<Device> Devices => Set<Device>();
        public DbSet<AlarmRule> AlarmRules => Set<AlarmRule>();
        public DbSet<Alarm> Alarms => Set<Alarm>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<ReadingRecord> Readings => Set<ReadingRecord>();

        public PlantPulseDbContext(DbContextOptions<PlantPulseDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlantPulseDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var aggregates = ChangeTracker.Entries<AggregateRoot>()
                .Select(e => e.Entity)
                .Where(a => a.DomainEvents.Count > 0)
                .ToList();

            foreach (var aggregate in aggregates)
            {
                foreach (var domainEvent in aggregate.DomainEvents)
                    OutboxMessages.Add(OutboxMessage.From(domainEvent));

                aggregate.ClearDomainEvents();
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}