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
                int page,
                ulong pageSize,
                IImageSearchService searchService,
                CancellationToken cancellationToken) =>
            {
                var offset = (ulong)(page - 1) * pageSize;
                var result = await searchService.SearchImagesAsync(
                    groupId, query, pageSize, offset, cancellationToken);
                return Results.Ok(result);
            });

        return app;
    }
}