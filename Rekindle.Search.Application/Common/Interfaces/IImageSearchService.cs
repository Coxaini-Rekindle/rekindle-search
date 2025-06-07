using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Common.Interfaces;

public interface IImageSearchService
{
    Task SaveImageAsync(FamilyPhoto photoData, Stream image, string contentType);

    Task<IEnumerable<FamilyPhoto>> SearchImagesAsync(
        Guid groupId,
        string query,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken ctx = default);
}