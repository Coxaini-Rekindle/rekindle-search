using Qdrant.Client;
using Qdrant.Client.Grpc;
using Rekindle.Search.Application.Common.Repositories;
using Rekindle.Search.Domain;

namespace Rekindle.Search.Infrastructure.Qdrant.Repositories;

public class FamilyPhotosRepository : IFamilyPhotosRepository
{
    private readonly QdrantClient _qdrantClient;

    public FamilyPhotosRepository(QdrantClient qdrantClient)
    {
        _qdrantClient = qdrantClient;
    }

    public async Task InsertPhoto(FamilyPhoto photo, ReadOnlyMemory<float> embedding)
    {
        await _qdrantClient.UpsertAsync(CollectionConstants.FamilyPhotos, new List<PointStruct>()
        {
            new()
            {
                Id = photo.FileId,
                Vectors = embedding.ToArray(),
                Payload =
                {
                    { "groupId", photo.GroupId.ToString() },
                    { "memoryId", photo.MemoryId.ToString() },
                    { "postId", photo.PostId.ToString() },
                    { "content", photo.Content },
                    { "title", photo.Title },
                    { "userId", photo.PublisherUserId.ToString() },
                    { "createdAt", photo.CreatedAt.ToString("o") }
                }
            }
        });
    }

    public async Task<IReadOnlyList<FamilyPhoto>> SearchPhotos(
        Guid groupId,
        ReadOnlyMemory<float> embedding,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default)
    {
        var response = await _qdrantClient.QueryAsync(
            collectionName: CollectionConstants.FamilyPhotos,
            filter: new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                            { Key = "groupId", Match = new Match { Keyword = groupId.ToString() } }
                    }
                }
            },
            query: embedding.ToArray(),
            limit: limit,
            offset: offset,
            cancellationToken: cancellationToken);

        return response.Where(point => point.Score > 0.45).Select(point => new FamilyPhoto
        {
            FileId = Guid.Parse(point.Id.Uuid),
            GroupId = Guid.Parse(point.Payload["groupId"].StringValue),
            MemoryId = Guid.Parse(point.Payload["memoryId"].StringValue),
            PostId = Guid.Parse(point.Payload["postId"].StringValue),
            Content = point.Payload["content"].StringValue,
            Title = point.Payload["title"].StringValue,
            PublisherUserId = Guid.Parse(point.Payload["userId"].StringValue),
            CreatedAt = DateTime.Parse(point.Payload["createdAt"].StringValue, null,
                System.Globalization.DateTimeStyles.RoundtripKind)
        }).ToList();
    }
}