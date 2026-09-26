namespace HomeControl.Domain.Devices.Exceptions;

public sealed class DeviceCannotBeTurnedOffException : Exception
{
    public DeviceCannotBeTurnedOffException(Guid deviceId, DeviceType deviceType)
        : base("Only lights can be turned off.")
    {
        DeviceId = deviceId;
        DeviceType = deviceType;
    }

    public Guid DeviceId { get; }

    public DeviceType DeviceType { get; }
}
