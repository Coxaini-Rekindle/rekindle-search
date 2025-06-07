using Rebus.Handlers;
using Rekindle.Memories.Contracts;
using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Storage.Interfaces;
using Rekindle.Search.Domain;

namespace Rekindle.Search.Application.Posts.EventHandlers;

public class PostCreatedEventHandler : IHandleMessages<PostCreatedEvent>
{
    private readonly IFileStorage _fileStorage;
    private readonly IImageSearchService _imageSearchService;

    public PostCreatedEventHandler(IFileStorage fileStorage, IImageSearchService imageSearchService)
    {
        _fileStorage = fileStorage;
        _imageSearchService = imageSearchService;
    }

    public async Task Handle(PostCreatedEvent message)
    {
        var imageTasks = message.Images.Select(i => _fileStorage.GetAsync(i));
        var images = await Task.WhenAll(imageTasks);

        foreach (var image in images)
        {
            var photoData = new FamilyPhoto
            {
                FileId = message.Id,
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