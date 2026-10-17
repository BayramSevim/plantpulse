using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlantPulse.Application.Readings;

public record ReadingData(string DeviceCode, double Value, DateTime Timestamp);