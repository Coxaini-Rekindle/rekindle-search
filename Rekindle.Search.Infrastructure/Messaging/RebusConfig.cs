using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rekindle.Memories.Contracts;
using Rekindle.Search.Application;

namespace Rekindle.Search.Infrastructure.Messaging;

/// <summary>
/// Configuration for Rebus message bus
/// </summary>
public static class RebusConfig
{
    /// <summary>
    /// Registers Rebus with RabbitMQ transport in the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection with Rebus registered</returns>
    public static IServiceCollection AddRebusMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
                                       ?? "amqp://guest:guest@localhost:5672";

        var serviceName = configuration["ServiceName"];

        services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq(rabbitMqConnectionString, serviceName))
                .Logging(l => l.ColoredConsole()),
            onCreated: async bus =>
            {
                await bus.Subscribe<PostCreatedEvent>();
            }
        );
        services.AutoRegisterHandlersFromAssemblyOf<IApplicationAssemblyMarker>();
        //services.AddTransient<IEventPublisher, RebusEventPublisher>();

        return services;
    }
}

public interface IInfrastructureAssemblyMarker
{
}