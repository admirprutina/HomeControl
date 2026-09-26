using HomeControl.Domain.Devices;

namespace HomeControl.Application.Features.Devices.Commands.RegisterDevice;

public sealed record RegisterDeviceCommandResult(Guid DeviceId, string Name, DeviceType Type);
