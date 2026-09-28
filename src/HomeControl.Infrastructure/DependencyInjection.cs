using HomeControl.Application.Abstractions.Connections;
using HomeControl.Application.Abstractions.Messaging;
using HomeControl.Application.Abstractions.Persistence;
using HomeControl.Domain.Devices;
using HomeControl.Domain.Devices.Events;
using HomeControl.Infrastructure.Connections;
using HomeControl.Infrastructure.Messaging;
using HomeControl.Infrastructure.Messaging.Kafka;
using HomeControl.Infrastructure.Persistence;
using HomeControl.Infrastructure.Persistence.Documents;
using HomeControl.Infrastructure.Persistence.Projections;
using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControl.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Marten");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing PostgreSQL connection string: ConnectionStrings:Marten.");
        }

        services.AddMarten(options =>
        {
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
            options.Schema.For<DeviceDocument>().DocumentAlias("device");
            options.Schema.For<Device>().DocumentAlias("device_aggregate");
            options.Events.AddEventType(typeof(DeviceRegistered));
            options.Events.AddEventType(typeof(LightTurnedOn));
            options.Events.AddEventType(typeof(LightTurnedOff));
            options.Projections.Add<DeviceAggregateProjection>(ProjectionLifecycle.Live);
            options.Projections.Add<DeviceDocumentProjection>(ProjectionLifecycle.Inline);
        }).UseLightweightSessions();

        services.AddScoped<IDeviceEventStore, MartenDeviceEventStore>();
        services.AddScoped<IDeviceRepository, MartenDeviceRepository>();
        services.AddSingleton(RabbitMqSettings.FromConfiguration(configuration));
        services.AddSingleton<IDeviceEventPublisher, RabbitMqDeviceEventPublisher>();
        services.AddSingleton(KafkaSettings.FromConfiguration(configuration));
        services.AddSingleton<KafkaTopicInitializer>();
        services.AddSingleton<IDeviceTelemetryPublisher, KafkaDeviceTelemetryPublisher>();
        services.AddSingleton<WifiConnectionStrategy>();
        services.AddSingleton<ZigbeeConnectionStrategy>();
        services.AddSingleton<BluetoothConnectionStrategy>();
        services.AddSingleton<IDeviceConnectionStrategyResolver, DeviceConnectionStrategyResolver>();

        return services;
    }
}
