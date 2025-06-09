using Google.Protobuf.Collections;
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
                    { "createdAt", photo.CreatedAt.ToString("o") },
                    { "participants", photo.Participants.Select(u => u.ToString()).ToArray() }
                }
            }
        });
    }

    public async Task<FamilyPhoto?> FindByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var response = await _qdrantClient.QueryAsync(
            collectionName: CollectionConstants.FamilyPhotos,
            query: fileId,
            cancellationToken: cancellationToken);

        var point = response.FirstOrDefault();
        if (point == null)
        {
            return null;
        }

        return new FamilyPhoto
        {
            FileId = Guid.Parse(point.Id.Uuid),
            GroupId = Guid.Parse(point.Payload["groupId"].StringValue),
            MemoryId = Guid.Parse(point.Payload["memoryId"].StringValue),
            PostId = Guid.Parse(point.Payload["postId"].StringValue),
            Content = point.Payload["content"].StringValue,
            Title = point.Payload["title"].StringValue,
            PublisherUserId = Guid.Parse(point.Payload["userId"].StringValue),
            CreatedAt = DateTime.Parse(point.Payload["createdAt"].StringValue, null,
                System.Globalization.DateTimeStyles.RoundtripKind),
            Participants = point.Payload["participants"].ListValue.Values
                .Select(v => Guid.Parse(v.StringValue))
                .ToList()
        };
    }

    public async Task<IEnumerable<FamilyPhoto>> FindByParticipantIdAsync(
        Guid groupId,
        Guid participant,
        CancellationToken cancellationToken = default)
    {
        var filter = new Filter
        {
            Must =
            {
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "groupId",
                        Match = new Match { Keyword = groupId.ToString() }
                    }
                },
                new Condition
                {
                    Field = new FieldCondition
                    {
                        Key = "participants",
                        Match = new Match { Keyword = participant.ToString() }
                    }
                }
            }
        };

        var response = await _qdrantClient.QueryAsync(
            collectionName: CollectionConstants.FamilyPhotos,
            filter: filter,
            cancellationToken: cancellationToken);

        return response.Select(point => new FamilyPhoto
        {
            FileId = Guid.Parse(point.Id.Uuid),
            GroupId = Guid.Parse(point.Payload["groupId"].StringValue),
            MemoryId = Guid.Parse(point.Payload["memoryId"].StringValue),
            PostId = Guid.Parse(point.Payload["postId"].StringValue),
            Content = point.Payload["content"].StringValue,
            Title = point.Payload["title"].StringValue,
            PublisherUserId = Guid.Parse(point.Payload["userId"].StringValue),
            CreatedAt = DateTime.Parse(point.Payload["createdAt"].StringValue, null,
                System.Globalization.DateTimeStyles.RoundtripKind),
            Participants = point.Payload["participants"].ListValue.Values
                .Select(v => Guid.Parse(v.StringValue))
                .ToList()
        }).ToList();
    }

    public async Task ReplacePhotoPayload(
        FamilyPhoto photo,
        CancellationToken cancellationToken = default)
    {
        await _qdrantClient.SetPayloadAsync(
            collectionName: CollectionConstants.FamilyPhotos,
            ids: [photo.FileId],
            payload: new Dictionary<string, Value>
            {
                { "groupId", photo.GroupId.ToString() },
                { "memoryId", photo.MemoryId.ToString() },
                { "postId", photo.PostId.ToString() },
                { "content", photo.Content },
                { "title", photo.Title },
                { "userId", photo.PublisherUserId.ToString() },
                { "createdAt", photo.CreatedAt.ToString("o") },
                { "participants", photo.Participants.Select(u => u.ToString()).ToArray() }
            },
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<FamilyPhoto>> SearchPhotos(
        Guid groupId,
        IEnumerable<Guid> participants,
        ReadOnlyMemory<float>? embedding,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default)
    {
        var participantIds = participants.Select(p => p.ToString()).ToArray();

        // Create the groupId condition
        var mustConditions = new RepeatedField<Condition>
        {
            new Condition
            {
                Field = new FieldCondition
                {
                    Key = "groupId",
                    Match = new Match { Keyword = groupId.ToString() }
                }
            }
        };

        foreach (var id in participantIds)
        {
            mustConditions.Add(new Condition
            {
                Field = new FieldCondition
                {
                    Key = "participants",
                    Match = new Match { Keyword = id }
                }
            });
        }

        var filter = new Filter
        {
            Must = { mustConditions }
        };

        IReadOnlyList<ScoredPoint> response;
        if (embedding is not null)
        {
            response = await _qdrantClient.QueryAsync(
                collectionName: CollectionConstants.FamilyPhotos,
                filter: filter,
                query: embedding.Value.ToArray(),
                limit: limit,
                offset: offset,
                cancellationToken: cancellationToken);
        }
        else
        {
            response = await _qdrantClient.QueryAsync(
                collectionName: CollectionConstants.FamilyPhotos,
                filter: filter,
                limit: limit,
                offset: offset,
                cancellationToken: cancellationToken);
        }

        return response.Where(point => point.Score > 0.45 || embedding is null).Select(point => new FamilyPhoto
        {
            FileId = Guid.Parse(point.Id.Uuid),
            GroupId = Guid.Parse(point.Payload["groupId"].StringValue),
            MemoryId = Guid.Parse(point.Payload["memoryId"].StringValue),
            PostId = Guid.Parse(point.Payload["postId"].StringValue),
            Content = point.Payload["content"].StringValue,
            Title = point.Payload["title"].StringValue,
            PublisherUserId = Guid.Parse(point.Payload["userId"].StringValue),
            CreatedAt = DateTime.Parse(point.Payload["createdAt"].StringValue, null,
                System.Globalization.DateTimeStyles.RoundtripKind),
            Participants = point.Payload["participants"].ListValue.Values
                .Select(v => Guid.Parse(v.StringValue))
                .ToList()
        }).ToList();
    }
}