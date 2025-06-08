using Rebus.Bus;
using Rekindle.Search.Application.Common.Messaging;
using Rekindle.Search.Contracts;

namespace Rekindle.Search.Infrastructure.Messaging;

public class RebusEventPublisher : IEventPublisher
{
    private readonly IBus _bus;

    public RebusEventPublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : SearchEvent
    {
        await _bus.Publish(@event);
    }
}