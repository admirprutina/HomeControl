namespace HomeControl.Domain.Devices.Exceptions;

public sealed class DeviceCannotBeTurnedOnException : Exception
{
    public DeviceCannotBeTurnedOnException(Guid deviceId, DeviceType deviceType)
        : base("Only lights can be turned on.")
    {
        DeviceId = deviceId;
        DeviceType = deviceType;
    }

    public Guid DeviceId { get; }

    public DeviceType DeviceType { get; }
}
