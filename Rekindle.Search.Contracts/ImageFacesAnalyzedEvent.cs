namespace Rekindle.Search.Contracts;

public class ImageFacesAnalyzedEvent : SearchEvent
{
    public Guid GroupId { get; set; }
    public Guid PostId { get; set; }
    public Guid ImageId { get; set; }
    public IEnumerable<Guid> UserIds { get; set; }
    public IEnumerable<Guid> TempUserIds { get; set; }
}