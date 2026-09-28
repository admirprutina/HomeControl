using System.Globalization;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace HomeControl.Infrastructure.Messaging.Kafka;

// Explicit local-development setup. No background topic management or partition changes.
public sealed class KafkaTopicInitializer(KafkaSettings settings) : IDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            using var admin = new AdminClientBuilder(settings.CreateClientConfig()).Build();
            try
            {
                await admin.CreateTopicsAsync(
                    [new TopicSpecification
                    {
                        Name = KafkaTopology.DeviceTelemetryTopic,
                        NumPartitions = KafkaTopology.PartitionCount,
                        ReplicationFactor = KafkaTopology.ReplicationFactor,
                        Configs = new Dictionary<string, string>
                        {
                            ["cleanup.policy"] = "delete",
                            ["retention.ms"] = KafkaTopology.RetentionMilliseconds.ToString(CultureInfo.InvariantCulture)
                        }
                    }],
                    new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) })
                    .WaitAsync(cancellationToken);
            }
            catch (CreateTopicsException exception) when (
                exception.Results.All(result => result.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                // API and worker may start concurrently; an existing topic is expected.
            }

            cancellationToken.ThrowIfCancellationRequested();
            var topic = admin.GetMetadata(KafkaTopology.DeviceTelemetryTopic, TimeSpan.FromSeconds(10))
                .Topics.Single();
            if (topic.Error.IsError)
            {
                throw new KafkaException(topic.Error);
            }

            if (topic.Partitions.Count != KafkaTopology.PartitionCount)
            {
                throw new KafkaException(new Error(ErrorCode.Local_InvalidArg,
                    $"Topic {topic.Topic} must have {KafkaTopology.PartitionCount} partitions; found {topic.Partitions.Count}."));
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public void Dispose() => _initializationLock.Dispose();
}
