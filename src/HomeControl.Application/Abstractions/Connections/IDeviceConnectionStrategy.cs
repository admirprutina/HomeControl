using HomeControl.Domain.Devices;

namespace HomeControl.Application.Abstractions.Connections;

public interface IDeviceConnectionStrategy
{
    ConnectionTestResult TestConnection(Device device);
}
