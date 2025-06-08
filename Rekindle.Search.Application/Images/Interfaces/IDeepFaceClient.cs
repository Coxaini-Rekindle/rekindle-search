using Rekindle.Search.Application.Images.Models;

namespace Rekindle.Search.Application.Images.Interfaces;

public interface IDeepFaceClient
{
    Task<FaceAddResponse> AddFacesFromImageAsync(
        Guid groupId,
        Stream imageStream,
        CancellationToken cancellationToken = default);

    Task<Stream> GetLatestFaceByUserIdAsync(Guid groupId, Guid userId,
        CancellationToken cancellationToken = default);

    Task MergeUsers(Guid groupId, IEnumerable<Guid> sourceUserIds, Guid targetUserId,
        CancellationToken cancellationToken = default);
}