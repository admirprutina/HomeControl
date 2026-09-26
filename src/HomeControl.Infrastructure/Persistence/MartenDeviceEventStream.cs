using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Application.Features.Devices.Exceptions;
using HomeControl.Domain.Devices;
using JasperFx;
using JasperFx.Events;
using Marten;

namespace HomeControl.Infrastructure.Persistence;

internal sealed class MartenDeviceEventStream(
    Guid deviceId,
    IEventStream<Device> stream,
    IDocumentSession session) : IDeviceEventStream
{
    public Device Aggregate => stream.Aggregate!;

    public void Append(object @event)
    {
        stream.AppendOne(@event);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            throw new DeviceConcurrencyException(deviceId);
        }
    }
}
