using Rebus.Handlers;
using Rekindle.Memories.Contracts;

namespace Rekindle.Search.Application.Posts.EventHandlers;

public class PostCreatedEventHandler : IHandleMessages<PostCreatedEvent>
{
    public Task Handle(PostCreatedEvent message)
    {
        throw new NotImplementedException();
    }
}