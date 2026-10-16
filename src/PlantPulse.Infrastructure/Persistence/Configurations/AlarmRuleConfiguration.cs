using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;

namespace PlantPulse.Infrastructure.Persistence.Configurations
{
    public class AlarmRuleConfiguration : IEntityTypeConfiguration<AlarmRule>
    {
        public void Configure(EntityTypeBuilder<AlarmRule> builder)
        {
            builder.ToTable("alarm_rules");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();

            builder.HasOne<Device>()
                    .WithMany()
                    .HasForeignKey(r => r.DeviceId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.ComplexProperty(r => r.Threshold, t =>
            {
                t.Property(x => x.Limit).HasColumnName("threshold_limit");
                t.Property(x => x.Direction)
                    .HasConversion<string>()
                    .HasMaxLength(10)
                    .HasColumnName("threshold_direction");
                t.Property(x => x.Hysteresis).HasColumnName("threshold_hysteresis");
            });

            builder.Property(r => r.Duration).IsRequired();

            builder.Property(r => r.Severity)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(r => r.IsEnabled);
        }
    }
}
