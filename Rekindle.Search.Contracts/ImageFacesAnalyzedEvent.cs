namespace Rekindle.Search.Contracts;

public class ImageFacesAnalyzedEvent : SearchEvent
{
    public Guid PostId { get; set; }
    public IEnumerable<Guid> UserIds { get; set; }
    public IEnumerable<Guid> TempUserIds { get; set; }
}