using HomeControl.Domain.Devices;

namespace HomeControl.Application.Abstractions.Persistence;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken);
}
