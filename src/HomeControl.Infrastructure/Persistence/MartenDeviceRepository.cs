using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Domain.Devices;
using HomeControl.Infrastructure.Persistence.Documents;
using Marten;

namespace HomeControl.Infrastructure.Persistence;

public sealed class MartenDeviceRepository(IDocumentSession session) : IDeviceRepository
{
    public async Task<Device?> GetByIdAsync(Guid deviceId, CancellationToken cancellationToken)
    {
        var document = await session.LoadAsync<DeviceDocument>(deviceId, cancellationToken);

        return document is null
            ? null
            : Device.Rehydrate(document.Id, document.Name, document.Type, document.IsOn);
    }
}
