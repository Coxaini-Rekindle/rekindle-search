namespace Rekindle.Search.Domain;

public class FamilyPhoto
{
    public Guid FileId { get; set; }
    public Guid GroupId { get; set; }
    public Guid MemoryId { get; set; }
    public Guid PostId { get; set; }
    public Guid PublisherUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Guid> Participants { get; set; } = new();
}