using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace HomeControl.Infrastructure.Messaging.Kafka;

public sealed record KafkaSettings(
    string BootstrapServers,
    SecurityProtocol SecurityProtocol,
    SaslMechanism? SaslMechanism,
    string? SaslUsername,
    string? SaslPassword,
    string? SslCaLocation)
{
    public static KafkaSettings FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("Kafka");
        var servers = section["BootstrapServers"];
        if (string.IsNullOrWhiteSpace(servers))
        {
            throw new InvalidOperationException("Kafka:BootstrapServers must be configured.");
        }

        return new KafkaSettings(
            servers,
            section.GetValue<SecurityProtocol?>("SecurityProtocol") ?? Confluent.Kafka.SecurityProtocol.Plaintext,
            section.GetValue<SaslMechanism?>("SaslMechanism"),
            section["SaslUsername"],
            section["SaslPassword"],
            section["SslCaLocation"]);
    }

    public ClientConfig CreateClientConfig() => new()
    {
        BootstrapServers = BootstrapServers,
        SecurityProtocol = SecurityProtocol,
        SaslMechanism = SaslMechanism,
        SaslUsername = SaslUsername,
        SaslPassword = SaslPassword,
        SslCaLocation = SslCaLocation
    };
}
