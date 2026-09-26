using HomeControl.Application.Abstractions.Connections;
using HomeControl.Domain.Devices;

namespace HomeControl.Infrastructure.Connections;

public sealed class WifiConnectionStrategy : IDeviceConnectionStrategy
{
    public ConnectionTestResult TestConnection(Device device) => new("WiFi", true);
}
