namespace Rekindle.Search.Contracts;

public abstract class SearchEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}