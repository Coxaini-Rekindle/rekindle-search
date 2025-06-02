using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rekindle.Search.Application.Storage.Interfaces;
using Rekindle.Search.Infrastructure.Messaging;
using Rekindle.Search.Infrastructure.Storage;

namespace Rekindle.Search.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRebusMessageBus(configuration);
        services.AddFileStorage(configuration);

        return services;
    }
    
    private static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));

        return services;
    }
}