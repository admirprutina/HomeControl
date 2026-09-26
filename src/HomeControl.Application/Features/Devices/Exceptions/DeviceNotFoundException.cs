namespace HomeControl.Application.Features.Devices.Exceptions;

public sealed class DeviceNotFoundException : Exception
{
    public DeviceNotFoundException(Guid deviceId)
        : base($"Device with id '{deviceId}' was not found.")
    {
        DeviceId = deviceId;
    }

    public Guid DeviceId { get; }
}
