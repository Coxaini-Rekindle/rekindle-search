using Microsoft.Extensions.Logging;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Common.Repositories;
using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Images;

public class ImageSearchService : IImageSearchService
{
    private readonly IImageDescriptor _imageDescriptor;
    private readonly IVectorEmbeddingGenerator _vectorEmbeddingGenerator;
    private readonly IFamilyPhotosRepository _familyPhotosRepository;
    private readonly ILogger<ImageSearchService> _logger;

    public ImageSearchService(IImageDescriptor imageDescriptor, IVectorEmbeddingGenerator vectorEmbeddingGenerator,
        IFamilyPhotosRepository familyPhotosRepository, ILogger<ImageSearchService> logger)
    {
        _imageDescriptor = imageDescriptor;
        _vectorEmbeddingGenerator = vectorEmbeddingGenerator;
        _familyPhotosRepository = familyPhotosRepository;
        _logger = logger;
    }

    public async Task SaveImageAsync(FamilyPhoto photoData, Stream image, string contentType)
    {
        // Step 1: Describe the image to get a text description
        var description = await _imageDescriptor.DescribeImageAsync(image, contentType);

        // Step 2: Generate a vector embedding from the description
        var text = photoData.Title + " " + photoData.Content + " " + description;
        var embedding = await _vectorEmbeddingGenerator.GenerateEmbeddingAsync(text);

        // Step 3: Save the image and its embedding to the database
        await _familyPhotosRepository.InsertPhoto(photoData, embedding);

        _logger.LogInformation("Saved image with ID {PhotoId} and embedding to the database", photoData.FileId);
    }

    public async Task<IEnumerable<FamilyPhoto>> SearchImagesAsync(
        Guid groupId,
        IEnumerable<Guid> participants,
        string query,
        ulong limit = 10,
        ulong offset = 0,
        CancellationToken ctx = default)
    {
        ReadOnlyMemory<float>? embedding = null;
        // Step 1: Generate a vector embedding from the search query
        if (!string.IsNullOrWhiteSpace(query))
            embedding = await _vectorEmbeddingGenerator.GenerateEmbeddingAsync(query, ctx);

        // Step 2: Search for similar images in the database
        var results = await _familyPhotosRepository.SearchPhotos(groupId, participants, embedding, limit, offset, ctx);

        _logger.LogInformation("Found {Count} images matching query '{Query}'", results.Count, query);

        return results;
    }
}