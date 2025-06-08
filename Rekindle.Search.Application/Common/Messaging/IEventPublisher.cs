using Rekindle.Search.Contracts;

namespace Rekindle.Search.Application.Common.Messaging;

/// <summary>
/// Interface for publishing events to the message bus
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the message bus
    /// </summary>
    /// <param name="event">The event to publish</param>
    /// <typeparam name="TEvent">The type of event</typeparam>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : SearchEvent;
}