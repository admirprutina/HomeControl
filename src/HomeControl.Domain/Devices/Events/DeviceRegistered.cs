namespace HomeControl.Domain.Devices.Events;

public sealed record DeviceRegistered(
    Guid DeviceId,
    string Name,
    DeviceType Type);
