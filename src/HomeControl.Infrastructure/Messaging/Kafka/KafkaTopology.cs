namespace HomeControl.Infrastructure.Messaging.Kafka;

public static class KafkaTopology
{
    public const string DeviceTelemetryTopic = "homecontrol.device-telemetry";
    public const string AnalyticsConsumerGroup = "homecontrol.analytics";
    public const int PartitionCount = 3;
    public const short ReplicationFactor = 1;
    public const long RetentionMilliseconds = 7L * 24 * 60 * 60 * 1000;
}
