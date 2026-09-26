using FluentValidation;
using HomeControl.Application.Common.Behaviors;
using HomeControl.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace HomeControl.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var handlerContracts = new[] { typeof(IRequestHandler<,>), typeof(IRequestHandler<>) };
        var handlers = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
            .SelectMany(implementationType => implementationType.GetInterfaces()
                .Where(serviceType => serviceType.IsGenericType
                    && handlerContracts.Contains(serviceType.GetGenericTypeDefinition()))
                .Select(serviceType => (ServiceType: serviceType, ImplementationType: implementationType)))
            .ToArray();

        var duplicate = handlers
            .GroupBy(handler => handler.ServiceType.GenericTypeArguments[0])
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Multiple handlers found for request {duplicate.Key.FullName}.");
        }

        services.AddScoped<ISender, Mediator>();

        foreach (var (serviceType, implementationType) in handlers)
        {
            services.AddScoped(serviceType, implementationType);
        }

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Scoped);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));

        return services;
    }
}
