namespace HomeControl.Infrastructure.Messaging;

public static class RabbitMqTopology
{
    public const string Exchange = "homecontrol.events";
    public const string AuditQueue = "homecontrol.device-audit";
    public const string AuditBinding = "device.#";
    public const string LightTurnedOn = "device.light.turned-on";
    public const string LightTurnedOff = "device.light.turned-off";
}
