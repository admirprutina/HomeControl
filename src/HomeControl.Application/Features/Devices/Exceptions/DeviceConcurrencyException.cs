namespace HomeControl.Application.Features.Devices.Exceptions;

public sealed class DeviceConcurrencyException : Exception
{
    public DeviceConcurrencyException(Guid deviceId)
        : base("The device changed while the operation was being processed.")
    {
        DeviceId = deviceId;
    }

    public Guid DeviceId { get; }
}
