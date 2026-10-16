using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantPulse.Infrastructure.Outbox;

namespace PlantPulse.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).ValueGeneratedNever();

            builder.Property(m => m.Type).HasMaxLength(200).IsRequired();

            builder.Property(r => r.Payload).HasColumnType("jsonb").IsRequired();

            builder.Property(r => r.OccurredAt);

            builder.Property(r => r.ProcessedAt);

            builder.Property(r => r.AttemptCount);

            builder.Property(r => r.NextAttemptAt);

            builder.HasIndex(m => m.NextAttemptAt).HasFilter("\"ProcessedAt\" IS NULL");

            builder.Property(r => r.Error);

        }
    }
}