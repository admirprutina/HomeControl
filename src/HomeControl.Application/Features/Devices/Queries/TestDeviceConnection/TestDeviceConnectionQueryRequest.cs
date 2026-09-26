using HomeControl.Application.Messaging;

namespace HomeControl.Application.Features.Devices.Queries.TestDeviceConnection;

public sealed record TestDeviceConnectionQueryRequest(Guid DeviceId)
    : IRequest<TestDeviceConnectionQueryResult>;
