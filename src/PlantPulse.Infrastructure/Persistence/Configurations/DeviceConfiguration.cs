using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantPulse.Domain.Devices;

namespace PlantPulse.Infrastructure.Persistence.Configurations
{
    public class DeviceConfiguration : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable("devices");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id).ValueGeneratedNever();

            builder.Property(d => d.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(d => d.Code)
              .HasMaxLength(20)
              .IsRequired();

            builder.HasIndex(d => d.Code)
                .IsUnique();

            builder.Property(d => d.Type)
                   .HasConversion<string>()
                    .HasMaxLength(20);

            builder.Property(d => d.IsActive);
        }
    }
}