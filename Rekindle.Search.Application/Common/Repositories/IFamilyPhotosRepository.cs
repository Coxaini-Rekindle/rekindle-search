using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Common.Repositories;

public interface IFamilyPhotosRepository
{
    Task InsertPhoto(FamilyPhoto photo, ReadOnlyMemory<float> embedding);

    Task<IReadOnlyList<FamilyPhoto>> SearchPhotos(
        Guid groupId,
        ReadOnlyMemory<float> embedding,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken cancellationToken = default);
}