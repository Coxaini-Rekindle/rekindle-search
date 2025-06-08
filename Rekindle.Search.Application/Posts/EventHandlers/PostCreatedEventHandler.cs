using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rekindle.Memories.Contracts;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Common.Messaging;
using Rekindle.Search.Application.Images.Interfaces;
using Rekindle.Search.Application.Images.Models;
using Rekindle.Search.Application.Storage.Interfaces;
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
        var imageTasks = message.Images.Select(i => _fileStorage.GetAsync(i));
        var images = await Task.WhenAll(imageTasks);

        foreach (var image in images)
        {
            try
            {
                var faceAnalysis = await _deepFaceClient.AddFacesFromImageAsync(
                    message.GroupId,
                    image.Stream);

                await _eventPublisher.PublishAsync(new ImageFacesAnalyzedEvent
                {
                    PostId = message.PostId,
                    ImageId = image.FileId,
                    GroupId = message.GroupId,
                    UserIds = faceAnalysis.Faces.Where(f => f.RecognitionType == FaceRecognitionType.Recognized)
                        .Select(f => Guid.Parse(f.PersonId)),
                    TempUserIds = faceAnalysis.Faces
                        .Where(f => f.RecognitionType == FaceRecognitionType.TempUser)
                        .Select(f => Guid.Parse(f.PersonId))
                });
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Failed to analyze faces for post {PostId} in group {GroupId}",
                    message.PostId, message.GroupId);
            }

            var photoData = new FamilyPhoto
            {
                FileId = image.FileId,
                Content = message.Content,
                Title = message.Title,
                GroupId = message.GroupId,
                MemoryId = message.MemoryId,
                PostId = message.PostId,
                PublisherUserId = message.UserId,
                CreatedAt = message.OccurredOn,
            };

            await _imageSearchService.SaveImageAsync(
                photoData,
                image.Stream,
                image.ContentType);
        }
    }
}