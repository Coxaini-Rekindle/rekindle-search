using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Rekindle.Search.Infrastructure.Qdrant;

public class QdrantCollectionInitializer : IHostedService
{
    private readonly ILogger<QdrantCollectionInitializer> _logger;
    private readonly QdrantClient _qdrantClient;


    public QdrantCollectionInitializer(ILogger<QdrantCollectionInitializer> logger, QdrantClient qdrantClient)
    {
        _logger = logger;
        _qdrantClient = qdrantClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking if Qdrant collection '{Collection}' exists...",
            CollectionConstants.FamilyPhotos);

        var exists = await _qdrantClient.CollectionExistsAsync(CollectionConstants.FamilyPhotos, cancellationToken);
        if (exists)
        {
            _logger.LogInformation("Collection '{Collection}' already exists", CollectionConstants.FamilyPhotos);
            return;
        }

        _logger.LogInformation("Creating collection '{Collection}'...", CollectionConstants.FamilyPhotos);

        await _qdrantClient.CreateCollectionAsync(CollectionConstants.FamilyPhotos, vectorsConfig: new VectorParams()
        {
            Size = 1024,
            Distance = Distance.Cosine
        }, cancellationToken: cancellationToken);

        await _qdrantClient.CreatePayloadIndexAsync(
            CollectionConstants.FamilyPhotos,
            "groupId",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Collection '{Collection}' created successfully", CollectionConstants.FamilyPhotos);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}