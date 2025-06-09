using Rekindle.Search.Application.Common.Repositories;
using Rekindle.Search.Application.Images.Interfaces;
using Rekindle.Search.Application.Users.Interfaces;

namespace Rekindle.Search.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IDeepFaceClient _deepFaceClient;
    private readonly IFamilyPhotosRepository _familyPhotosRepository;

    public UserService(IDeepFaceClient deepFaceClient, IFamilyPhotosRepository familyPhotosRepository)
    {
        _deepFaceClient = deepFaceClient;
        _familyPhotosRepository = familyPhotosRepository;
    }

    public async Task MergeUsersAsync(Guid groupId, Guid sourceUserId, Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        if (sourceUserId == targetUserId)
        {
            return;
        }

        await _deepFaceClient.MergeUsers(groupId, [sourceUserId], targetUserId, cancellationToken);

        var photos = await _familyPhotosRepository.FindByParticipantIdAsync(
            groupId, sourceUserId, cancellationToken);

        foreach (var photo in photos)
        {
            photo.Participants.Remove(sourceUserId);
            photo.Participants.Add(targetUserId);

            await _familyPhotosRepository.ReplacePhotoPayload(photo, cancellationToken);
        }
    }
}