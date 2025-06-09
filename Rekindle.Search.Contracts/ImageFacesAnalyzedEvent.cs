namespace Rekindle.Search.Contracts;

public class ImageFacesAnalyzedEvent : SearchEvent
{
    public Guid GroupId { get; set; }
    public Guid PostId { get; set; }
    public Guid ImageId { get; set; }
    public IEnumerable<UserWithFace> Users { get; set; }
    public IEnumerable<UserWithFace> TempUser { get; set; }
}

public class UserWithFace
{
    public UserWithFace(Guid userId, Guid faceFileId)
    {
        UserId = userId;
        FaceFileId = faceFileId;
    }

    public Guid UserId { get; set; }
    public Guid FaceFileId { get; set; }
}