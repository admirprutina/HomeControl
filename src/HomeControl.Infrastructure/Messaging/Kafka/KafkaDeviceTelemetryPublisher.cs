using System.Text.Json;
using Confluent.Kafka;
using HomeControl.Application.Abstractions.Messaging;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using Microsoft.Extensions.Logging;

namespace HomeControl.Infrastructure.Messaging.Kafka;

public sealed class KafkaDeviceTelemetryPublisher : IDeviceTelemetryPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaTopicInitializer _topicInitializer;
    private readonly ILogger<KafkaDeviceTelemetryPublisher> _logger;

    public KafkaDeviceTelemetryPublisher(
        KafkaSettings settings,
        KafkaTopicInitializer topicInitializer,
        ILogger<KafkaDeviceTelemetryPublisher> logger)
    {
        _topicInitializer = topicInitializer;
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig(settings.CreateClientConfig())
        {
            Acks = Acks.All,
            MessageSendMaxRetries = 0,
            MaxInFlight = 1
        }).Build();
    }

    public async Task PublishAsync(DeviceTelemetryRecorded message, CancellationToken cancellationToken)
    {
        await _topicInitializer.InitializeAsync(cancellationToken);

        // Kafka hashes this stable key to a partition; ordering is per partition.
        var key = message.DeviceId.ToString("D");
        var value = JsonSerializer.Serialize(message);
        var delivery = await _producer.ProduceAsync(
            KafkaTopology.DeviceTelemetryTopic,
            new Message<string, string> { Key = key, Value = value },
            cancellationToken);

        _logger.LogInformation(
            "Telemetry published: Topic={Topic}, Key={Key}, Partition={Partition}, Offset={Offset}",
            delivery.Topic, key, delivery.Partition.Value, delivery.Offset.Value);
    }

    public void Dispose() => _producer.Dispose();
}
