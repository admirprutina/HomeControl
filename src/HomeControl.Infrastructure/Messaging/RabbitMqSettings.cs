using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace HomeControl.Infrastructure.Messaging;

public sealed record RabbitMqSettings(string HostName, int Port, string UserName, string Password)
{
    public static RabbitMqSettings FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("RabbitMq");
        var hostName = section["HostName"];
        var userName = section["UserName"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(hostName)
            || !int.TryParse(section["Port"], out var port)
            || port is < 1 or > 65535
            || string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "RabbitMq:HostName, Port, UserName, and Password must be configured.");
        }

        return new RabbitMqSettings(hostName, port, userName, password);
    }

    public ConnectionFactory CreateConnectionFactory() => new()
    {
        HostName = HostName,
        Port = Port,
        UserName = UserName,
        Password = Password
    };
}
