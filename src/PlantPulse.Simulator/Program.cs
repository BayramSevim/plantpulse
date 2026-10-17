using System.Text.Json;
using Confluent.Kafka;
using PlantPulse.Simulator;

const string topic = "plant.readings";
var interval = TimeSpan.FromSeconds(1);

var devices = new List<DeviceProfile>
{
    new("PRESS-01",  "MotorCurrent", BaseValue: 182, Noise: 3,   SpikeOffset: 35),
    new("PRESS-02",  "MotorCurrent", BaseValue: 190, Noise: 3,   SpikeOffset: 35),
    new("MIXER-01",  "MotorCurrent", BaseValue: 74,  Noise: 4,   SpikeOffset: 30),
    new("COND-01",   "Temperature",  BaseValue: 84,  Noise: 0.8, SpikeOffset: 10),
    new("DRYER-01",  "Temperature",  BaseValue: 88,  Noise: 0.8, SpikeOffset: 10),
    new("COOLER-01", "Temperature",  BaseValue: 31,  Noise: 0.5, SpikeOffset: 12),
    new("SILO-A",    "SiloLevel",    BaseValue: 72,  Noise: 0.3, SpikeOffset: -65),
    new("SILO-B",    "SiloLevel",    BaseValue: 48,  Noise: 0.3, SpikeOffset: -42),
    new("SILO-C",    "SiloLevel",    BaseValue: 30,  Noise: 0.3, SpikeOffset: -24),
    new("SILO-D",    "SiloLevel",    BaseValue: 63,  Noise: 0.3, SpikeOffset: -56),
};

var config = new ProducerConfig
{
    BootstrapServers = "localhost:9092",
    Acks = Acks.All,
    EnableIdempotence = true,
    LingerMs = 5
};

using var producer = new ProducerBuilder<string, string>(config).Build();
var generator = new ReadingGenerator();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

Console.WriteLine($"Simulator started: {devices.Count} devices -> '{topic}' every {interval.TotalSeconds} s. Ctrl+C to stop.");

while (!cts.IsCancellationRequested)
{
    var now = DateTime.UtcNow;

    foreach (var device in devices)
    {
        var reading = new ReadingMessage(device.Code, generator.Next(device), now);

        producer.Produce(topic, new Message<string, string>
        {
            Key = device.Code,
            Value = JsonSerializer.Serialize(reading)
        },
        report =>
        {
            if (report.Error.IsError)
                Console.WriteLine($"[ERROR] {device.Code}: {report.Error.Reason}");
        });
    }

    Console.WriteLine($"{now:HH:mm:ss} sent {devices.Count} readings");

    try { await Task.Delay(interval, cts.Token); }
    catch (TaskCanceledException) { }
}

producer.Flush(TimeSpan.FromSeconds(5));
Console.WriteLine("Simulator stopped.");
