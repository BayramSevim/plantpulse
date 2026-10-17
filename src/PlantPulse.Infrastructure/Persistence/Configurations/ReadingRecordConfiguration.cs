using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantPulse.Infrastructure.Readings;

namespace PlantPulse.Infrastructure.Persistence.Configurations
{
    public class ReadingRecordConfiguration : IEntityTypeConfiguration<ReadingRecord>
    {
        public void Configure(EntityTypeBuilder<ReadingRecord> builder)
        {
            builder.ToTable("readings");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).UseIdentityAlwaysColumn();

            builder.Property(r => r.DeviceCode).HasMaxLength(20).IsRequired();

            builder.Property(r => r.Value);

            builder.HasIndex(r => new { r.DeviceCode, r.Timestamp });
        }
    }
}