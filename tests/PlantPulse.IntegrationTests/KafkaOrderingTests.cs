using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;

namespace PlantPulse.IntegrationTests;

public sealed class KafkaOrderingTests : IAsyncLifetime
{
    private readonly KafkaContainer _container = new KafkaBuilder().Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task Messages_with_the_same_key_stay_on_one_partition_in_order()
    {
        const string topic = "plant.readings.test";
        var bootstrap = _container.GetBootstrapAddress();

        using (var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrap }).Build())
        {
            await admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification { Name = topic, NumPartitions = 3, ReplicationFactor = 1 }
            });
        }

        var devices = new[] { "PRESS-01", "PRESS-02", "SILO-C" };
        using (var producer = new ProducerBuilder<string, string>(new ProducerConfig
               {
                   BootstrapServers = bootstrap,
                   Acks = Acks.All,
                   EnableIdempotence = true
               }).Build())
        {
            for (var i = 1; i <= 10; i++)
                foreach (var device in devices)
                    await producer.ProduceAsync(topic, new Message<string, string> { Key = device, Value = i.ToString() });
        }

        var received = new List<ConsumeResult<string, string>>();
        using (var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig
               {
                   BootstrapServers = bootstrap,
                   GroupId = "ordering-test",
                   AutoOffsetReset = AutoOffsetReset.Earliest
               }).Build())
        {
            consumer.Subscribe(topic);
            var deadline = DateTime.UtcNow.AddSeconds(30);
            while (received.Count < 30 && DateTime.UtcNow < deadline)
            {
                var result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                if (result is not null)
                    received.Add(result);
            }
            consumer.Close();
        }

        Assert.Equal(30, received.Count);

        foreach (var device in devices)
        {
            var messages = received.Where(r => r.Message.Key == device).ToList();

            Assert.Single(messages.Select(m => m.Partition.Value).Distinct());

            Assert.Equal(Enumerable.Range(1, 10).Select(i => i.ToString()), messages.Select(m => m.Message.Value));
        }
    }
}
