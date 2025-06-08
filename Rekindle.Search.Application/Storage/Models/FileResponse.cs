namespace Rekindle.Search.Application.Storage.Models;

public record FileResponse(Guid FileId, Stream Stream, string ContentType);