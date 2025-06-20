using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rekindle.Memories.Contracts;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Common.Messaging;
using Rekindle.Search.Application.Images.Interfaces;
using Rekindle.Search.Application.Images.Models;
using Rekindle.Search.Application.Storage.Interfaces;
using Rekindle.Search.Application.Storage.Models;
using Rekindle.Search.Contracts;
using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Posts.EventHandlers;

public class PostCreatedEventHandler : IHandleMessages<PostCreatedEvent>
{
    private readonly IFileStorage _fileStorage;
    private readonly IImageSearchService _imageSearchService;
    private readonly IDeepFaceClient _deepFaceClient;
    private readonly ILogger<PostCreatedEventHandler> _logger;
    private readonly IEventPublisher _eventPublisher;

    public PostCreatedEventHandler(IFileStorage fileStorage, IImageSearchService imageSearchService,
        IDeepFaceClient deepFaceClient, ILogger<PostCreatedEventHandler> logger, IEventPublisher eventPublisher)
    {
        _fileStorage = fileStorage;
        _imageSearchService = imageSearchService;
        _deepFaceClient = deepFaceClient;
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(PostCreatedEvent message)
    {
        var images = await GetImagesAsync(message.Images);
        foreach (var image in images)
        {
            using var memoryStream = new MemoryStream();
            await image.Stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var clonedImage = image with { Stream = memoryStream };
            await ProcessImageAsync(message, clonedImage);
        }
    }

    private async Task<IEnumerable<FileResponse>> GetImagesAsync(IEnumerable<Guid> imageIds)
    {
        var imageTasks = imageIds.Select(id => _fileStorage.GetAsync(id));
        return await Task.WhenAll(imageTasks);
    }

    private async Task ProcessImageAsync(PostCreatedEvent message, FileResponse image)
    {
        using var faceAnalysisStream = new MemoryStream();
        using var searchServiceStream = new MemoryStream();

        image.Stream.Position = 0;
        await image.Stream.CopyToAsync(faceAnalysisStream);
        image.Stream.Position = 0;
        await image.Stream.CopyToAsync(searchServiceStream);

        faceAnalysisStream.Position = 0;
        searchServiceStream.Position = 0;

        var imageForFaces = image with { Stream = faceAnalysisStream };
        var imageForSearch = image with { Stream = searchServiceStream };

        var participantIds = await ProcessFaceAnalysisAsync(message, imageForFaces);
        await SaveImageToSearchServiceAsync(message, imageForSearch, participantIds);
    }

    private async Task<IEnumerable<Guid>> ProcessFaceAnalysisAsync(PostCreatedEvent message, FileResponse image)
    {
        try
        {
            var faceAnalysis = await _deepFaceClient.AddFacesFromImageAsync(
                message.GroupId,
                image.Stream);

            if (faceAnalysis.Faces?.Any() != true)
            {
                _logger.LogInformation("No faces detected in image {ImageId} for post {PostId}",
                    image.FileId, message.PostId);
                return [];
            }

            var faceAnalysisWithFiles = await ProcessDetectedFacesAsync(faceAnalysis.Faces);
            await PublishFaceAnalysisEventAsync(message, image, faceAnalysisWithFiles);

            var participantIds = faceAnalysisWithFiles
                .Select(f => Guid.Parse(f.Face.PersonId))
                .Distinct()
                .ToList();

            return participantIds;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to analyze faces for post {PostId} in group {GroupId}, image {ImageId}",
                message.PostId, message.GroupId, image.FileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during face analysis for post {PostId}, image {ImageId}",
                message.PostId, image.FileId);
        }

        return [];
    }

    private async Task<List<(FaceResponse Face, Guid FileId)>> ProcessDetectedFacesAsync(
        IEnumerable<FaceResponse> faces)
    {
        var faceAnalysisWithFiles = new List<(FaceResponse Face, Guid FileId)>();

        foreach (var face in faces)
        {
            try
            {
                var imageBytes = Convert.FromBase64String(face.FaceImageBase64);
                using var stream = new MemoryStream(imageBytes);

                var fileId = await _fileStorage.UploadAsync(stream, "image/jpeg");
                faceAnalysisWithFiles.Add((face, fileId));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process face image for face recognition");
            }
        }

        return faceAnalysisWithFiles;
    }

    private async Task PublishFaceAnalysisEventAsync(PostCreatedEvent message, FileResponse image,
        List<(FaceResponse Face, Guid FileId)> faceAnalysisWithFiles)
    {
        var recognizedUsers = faceAnalysisWithFiles
            .Where(f => f.Face.RecognitionType == FaceRecognitionType.Recognized)
            .Select(f => new UserWithFace(Guid.Parse(f.Face.PersonId), f.FileId));

        var tempUsers = faceAnalysisWithFiles
            .Where(f => f.Face.RecognitionType == FaceRecognitionType.TempUser)
            .Select(f => new UserWithFace(Guid.Parse(f.Face.PersonId), f.FileId));

        var faceAnalysisEvent = new ImageFacesAnalyzedEvent
        {
            PostId = message.PostId,
            ImageId = image.FileId,
            GroupId = message.GroupId,
            Users = recognizedUsers,
            TempUser = tempUsers,
        };

        await _eventPublisher.PublishAsync(faceAnalysisEvent);
    }

    private async Task SaveImageToSearchServiceAsync(PostCreatedEvent message, FileResponse image,
        IEnumerable<Guid> participantIds)
    {
        try
        {
            var photoData = CreateFamilyPhotoData(message, image, participantIds);
            await _imageSearchService.SaveImageAsync(photoData, image.Stream, image.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image {ImageId} to search service for post {PostId}",
                image.FileId, message.PostId);
        }
    }

    private static FamilyPhoto CreateFamilyPhotoData(PostCreatedEvent message, FileResponse image,
        IEnumerable<Guid> participantIds)
    {
        return new FamilyPhoto
        {
            FileId = image.FileId,
            Content = message.Content,
            Title = message.Title,
            GroupId = message.GroupId,
            MemoryId = message.MemoryId,
            PostId = message.PostId,
            PublisherUserId = message.UserId,
            CreatedAt = message.OccurredOn,
            Participants = participantIds.ToList()
        };
    }
}