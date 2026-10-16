using Microsoft.EntityFrameworkCore;
using PlantPulse.Domain.Alarms;
using PlantPulse.Domain.Devices;

namespace PlantPulse.Infrastructure.Persistence;

/// <summary>
/// Uygulama açılışında bekleyen migration'ları uygular ve veritabanı boşsa
/// örnek cihazları ve alarm kurallarını ekler.
/// Cihaz kodları Simulator'daki kodlarla birebir aynı olmalı.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(PlantPulseDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        // Daha önce eklendiyse tekrar ekleme (uygulama her açılışta bunu çalıştırır).
        if (await db.Devices.AnyAsync(ct))
            return;

        var duration = TimeSpan.FromSeconds(30);

        // (kod, ad, tip, eşik, önem)
        var seeds = new (string Code, string Name, DeviceType Type, Threshold Threshold, AlarmSeverity Severity)[]
        {
            ("PRESS-01",  "Pelet Presi 1", DeviceType.MotorCurrent, new Threshold(210, ThresholdDirection.Above, 5), AlarmSeverity.Critical),
            ("PRESS-02",  "Pelet Presi 2", DeviceType.MotorCurrent, new Threshold(210, ThresholdDirection.Above, 5), AlarmSeverity.Critical),
            ("MIXER-01",  "Mikser Motoru", DeviceType.MotorCurrent, new Threshold(95,  ThresholdDirection.Above, 5), AlarmSeverity.Warning),
            ("COND-01",   "Kondisyoner",   DeviceType.Temperature,  new Threshold(90,  ThresholdDirection.Above, 3), AlarmSeverity.Warning),
            ("DRYER-01",  "Kurutucu",      DeviceType.Temperature,  new Threshold(95,  ThresholdDirection.Above, 3), AlarmSeverity.Critical),
            ("COOLER-01", "Soğutucu",      DeviceType.Temperature,  new Threshold(40,  ThresholdDirection.Above, 2), AlarmSeverity.Warning),
            ("SILO-A",    "Silo A",        DeviceType.SiloLevel,    new Threshold(10,  ThresholdDirection.Below, 2), AlarmSeverity.Warning),
            ("SILO-B",    "Silo B",        DeviceType.SiloLevel,    new Threshold(10,  ThresholdDirection.Below, 2), AlarmSeverity.Warning),
            ("SILO-C",    "Silo C",        DeviceType.SiloLevel,    new Threshold(10,  ThresholdDirection.Below, 2), AlarmSeverity.Warning),
            ("SILO-D",    "Silo D",        DeviceType.SiloLevel,    new Threshold(10,  ThresholdDirection.Below, 2), AlarmSeverity.Warning),
        };

        foreach (var s in seeds)
        {
            var device = new Device(s.Code, s.Name, s.Type);
            db.Devices.Add(device);
            db.AlarmRules.Add(new AlarmRule(device.Id, s.Threshold, duration, s.Severity));
        }

        await db.SaveChangesAsync(ct);
    }
}
