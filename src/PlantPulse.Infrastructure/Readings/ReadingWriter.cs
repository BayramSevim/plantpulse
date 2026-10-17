using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Application.Abstractions;
using PlantPulse.Application.Readings;
using PlantPulse.Infrastructure.Persistence;

namespace PlantPulse.Infrastructure.Readings
{
    public class ReadingWriter : IReadingWriter
    {
        private readonly PlantPulseDbContext _db;

        public ReadingWriter(PlantPulseDbContext db)
        {
            _db = db;
        }

        public async Task WriteAsync(IReadOnlyCollection<ReadingData> readings, CancellationToken ct)
        {
            var records = readings.Select(r => new ReadingRecord
            {
                DeviceCode = r.DeviceCode,
                Value = r.Value,
                Timestamp = r.Timestamp
            });

            _db.Readings.AddRange(records);

            await _db.SaveChangesAsync(ct);
        }
    }
}