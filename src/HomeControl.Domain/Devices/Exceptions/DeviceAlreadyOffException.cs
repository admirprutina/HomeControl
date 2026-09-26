namespace HomeControl.Domain.Devices.Exceptions;

public sealed class DeviceAlreadyOffException : Exception
{
    public DeviceAlreadyOffException(Guid deviceId)
        : base("Device is already off.")
    {
        DeviceId = deviceId;
    }

    public Guid DeviceId { get; }
}
