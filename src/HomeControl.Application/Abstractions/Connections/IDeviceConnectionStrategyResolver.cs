using HomeControl.Domain.Devices;

namespace HomeControl.Application.Abstractions.Connections;

public interface IDeviceConnectionStrategyResolver
{
    IDeviceConnectionStrategy Resolve(DeviceType deviceType);
}
