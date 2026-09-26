using HomeControl.Domain.Devices;

namespace HomeControl.Application.Features.Devices.Queries.GetDeviceById;

public sealed record GetDeviceByIdQueryResult(Guid DeviceId, string Name, DeviceType Type, bool IsOn);
