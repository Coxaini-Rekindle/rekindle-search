namespace Rekindle.Search.Application.Images.Models;

public class LastUserFaceResponse
{
    public string GroupId { get; set; } = null!;
    public string PersonId { get; set; } = null!;
    public UserFaceImage Image { get; set; } = null!;
}

public class UserFaceImage
{
    public string ImageBase64 { get; set; } = null!;
}