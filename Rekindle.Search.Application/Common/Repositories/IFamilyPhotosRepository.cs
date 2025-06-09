using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Common.Repositories;

public interface IFamilyPhotosRepository
{
    Task ReplacePhotoPayload(
        FamilyPhoto photo,
        CancellationToken cancellationToken = default);

    Task InsertPhoto(FamilyPhoto photo, ReadOnlyMemory<float> embedding);

    Task<IEnumerable<FamilyPhoto>> FindByParticipantIdAsync(
        Guid groupId,
        Guid participant,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FamilyPhoto>> SearchPhotos(
        Guid groupId,
        IEnumerable<Guid> participants,
        ReadOnlyMemory<float>? embedding,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default);
}