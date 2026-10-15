using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantPulse.Domain.Alarms;

namespace PlantPulse.Infrastructure.Persistence.Configurations
{
    public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
    {
        public void Configure(EntityTypeBuilder<Alarm> builder)
        {
            builder.ToTable("alarms");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedNever();

            builder.Property(a => a.Severity)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(a => a.AcknowledgedBy).HasMaxLength(100);

            builder.Ignore(a => a.DomainEvents);

            builder.HasIndex(a => a.RuleId)
                .IsUnique()
                .HasFilter("\"Status\" = 'Active'");
        }
    }
}