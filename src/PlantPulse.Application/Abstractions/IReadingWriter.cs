using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlantPulse.Application.Readings;

namespace PlantPulse.Application.Abstractions
{
    public interface IReadingWriter
    {
        Task WriteAsync(IReadOnlyCollection<ReadingData> readings, CancellationToken ct);
    }
}