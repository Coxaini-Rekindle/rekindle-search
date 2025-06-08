using Rekindle.Search.Application.Common.Interfaces;

namespace Rekindle.Search.Api.Routes;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("groups/{groupId:guid}/search",
            async (
                Guid groupId,
                string query,
                ulong limit,
                ulong offset,
                IImageSearchService searchService,
                CancellationToken cancellationToken) =>
            {
                var results = (await searchService.SearchImagesAsync(
                    groupId, query, limit, offset, cancellationToken)).ToList();

                var groupResults = results
                    .Select(f => new SearchResultDto(
                        f.MemoryId,
                        f.FileId,
                        f.PostId,
                        f.PublisherUserId,
                        f.CreatedAt,
                        f.Title,
                        f.Content
                    ));

                return Results.Ok(groupResults);
            });

        return app;
    }

    private record SearchResultDto(
        Guid MemoryId,
        Guid PhotoId,
        Guid PostId,
        Guid PublisherUserId,
        DateTime CreatedAt,
        string Title,
        string Content
    );
}