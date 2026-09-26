namespace HomeControl.Application.Features.Devices.Queries.TestDeviceConnection;

public sealed record TestDeviceConnectionQueryResult(Guid DeviceId, string Protocol, bool IsSuccessful);
