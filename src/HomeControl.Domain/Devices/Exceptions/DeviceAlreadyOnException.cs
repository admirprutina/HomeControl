namespace HomeControl.Domain.Devices.Exceptions;

public sealed class DeviceAlreadyOnException : Exception
{
    public DeviceAlreadyOnException(Guid deviceId)
        : base("Device is already on.")
    {
        DeviceId = deviceId;
    }

    public Guid DeviceId { get; }
}
