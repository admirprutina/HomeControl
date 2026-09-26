using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Queries.GetDeviceById;

public sealed record GetDeviceByIdQueryRequest(Guid DeviceId)
    : IRequest<GetDeviceByIdQueryResult>;
