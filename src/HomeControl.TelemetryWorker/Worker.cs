using System.Text.Json;
using Confluent.Kafka;
using HomeControl.Application.Features.Devices.IntegrationEvents;
using HomeControl.Infrastructure.Messaging.Kafka;

namespace HomeControl.TelemetryWorker;

public sealed class Worker(
    ILogger<Worker> logger,
    KafkaSettings settings,
    KafkaTopicInitializer topicInitializer) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await topicInitializer.InitializeAsync(stoppingToken);
        using var consumer = new ConsumerBuilder<string, string>(new ConsumerConfig(settings.CreateClientConfig())
        {
            GroupId = KafkaTopology.AnalyticsConsumerGroup,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false,
            AllowAutoCreateTopics = false,
            // Only used if the group has no valid committed offset; restarting does not replay by itself.
            AutoOffsetReset = AutoOffsetReset.Earliest
        }).Build();

        consumer.Subscribe(KafkaTopology.DeviceTelemetryTopic);
        logger.LogInformation("Consuming Topic={Topic}, Group={Group}, with manual commits",
            KafkaTopology.DeviceTelemetryTopic, KafkaTopology.AnalyticsConsumerGroup);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var delivery = consumer.Consume(stoppingToken);
                try
                {
                    var message = JsonSerializer.Deserialize<DeviceTelemetryRecorded>(delivery.Message.Value);
                    if (message is null || message.DeviceId == Guid.Empty || message.OccurredAtUtc == default
                        || (message.TemperatureCelsius is null && message.PowerUsageWatts is null))
                    {
                        throw new JsonException("Invalid device telemetry message.");
                    }

                    // Processing is logging for this example. Only commit after it succeeds.
                    logger.LogInformation(
                        "Telemetry: DeviceId={DeviceId}, TemperatureCelsius={TemperatureCelsius}, PowerUsageWatts={PowerUsageWatts}, OccurredAtUtc={OccurredAtUtc}, Topic={Topic}, Partition={Partition}, Offset={Offset}",
                        message.DeviceId, message.TemperatureCelsius, message.PowerUsageWatts,
                        message.OccurredAtUtc, delivery.Topic, delivery.Partition.Value, delivery.Offset.Value);

                    stoppingToken.ThrowIfCancellationRequested();
                    // Commit(result) records Offset + 1: the next record for this group/partition.
                    consumer.Commit(delivery);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception,
                        "Telemetry processing/commit failed: Topic={Topic}, Partition={Partition}, Offset={Offset}. Stopping without advancing past this record.",
                        delivery.Topic, delivery.Partition.Value, delivery.Offset.Value);
                    // Continuing could commit a later offset in the same partition and skip this failure.
                    throw;
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown; the in-flight record can be delivered again.
        }
        finally
        {
            consumer.Close(); // Leaves the group; auto commit is disabled.
        }
    }
}
