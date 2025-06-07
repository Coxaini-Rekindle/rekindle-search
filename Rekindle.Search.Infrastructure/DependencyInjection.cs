using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OllamaSharp;
using Qdrant.Client;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Common.Repositories;
using Rekindle.Search.Application.Storage.Interfaces;
using Rekindle.Search.Infrastructure.Ai;
using Rekindle.Search.Infrastructure.Messaging;
using Rekindle.Search.Infrastructure.Qdrant;
using Rekindle.Search.Infrastructure.Qdrant.Repositories;
using Rekindle.Search.Infrastructure.Storage;

namespace Rekindle.Search.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRebusMessageBus(configuration);
        services.AddFileStorage(configuration);
        services.AddQdrantDatabase(configuration);
        services.AddAiServices(configuration);

        return services;
    }

    private static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IFileStorage, FileStorage>();
        services.AddSingleton(_ => new BlobServiceClient(configuration.GetConnectionString("BlobStorage")));

        return services;
    }

    private static IServiceCollection AddQdrantDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var qdrantUrl = configuration.GetConnectionString("Qdrant") ?? "http://localhost:6333";

        services.AddSingleton<QdrantClient>(_ =>
        {
            var client = new QdrantClient(new Uri(qdrantUrl));
            return client;
        });

        services.AddHostedService<QdrantCollectionInitializer>();

        services.AddScoped<IFamilyPhotosRepository, FamilyPhotosRepository>();

        return services;
    }

    private static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaUrl = configuration.GetConnectionString("Ollama") ?? "http://localhost:11434";

        services.AddChatClient(new OllamaApiClient(ollamaUrl, "llava:7b"));
        services.AddEmbeddingGenerator(new OllamaApiClient(ollamaUrl,
            "bge-m3"));

        services.AddSingleton<IImageDescriptor, ImageDescriptor>();
        services.AddSingleton<IVectorEmbeddingGenerator, VectorEmbeddingGenerator>();

        return services;
    }
}