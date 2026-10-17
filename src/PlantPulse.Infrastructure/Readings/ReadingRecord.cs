using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Infrastructure.Readings;

public class ReadingRecord
{
    public long Id { get; set; }
    public string DeviceCode { get; set; } = null!;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }

}