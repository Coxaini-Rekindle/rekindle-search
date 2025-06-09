using Rekindle.Search.Application.Common.Interfaces;
using Rekindle.Search.Application.Users.Interfaces;

namespace Rekindle.Search.Api.Routes;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("groups/{groupId:guid}/search",
            async (
                Guid groupId,
                [AsParameters] SearchParameters parameters,
                IImageSearchService searchService,
                CancellationToken cancellationToken) =>
            {
                var results = (await searchService.SearchImagesAsync(
                    groupId, parameters.Participants, parameters.Query, parameters.Limit, parameters.Offset,
                    cancellationToken)).ToList();

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

        app.MapPost("groups/{groupId:guid}/users/merge",
                async (
                    Guid groupId,
                    MergeUsersRequest request,
                    IUserService userService,
                    CancellationToken cancellationToken) =>
                {
                    await userService.MergeUsersAsync(
                        groupId,
                        request.UserId,
                        request.TargetUserId,
                        cancellationToken);

                    return Results.Ok();
                })
            .WithName("MergeUserImages")
            .WithDescription("Merge images from one user into another");

        return app;
    }

    public record SearchParameters
    {
        public string Query { get; init; } = string.Empty;
        public ulong Limit { get; init; } = 10;
        public ulong Offset { get; init; } = 0;
        public Guid[] Participants { get; init; } = [];
    }

    private record MergeUsersRequest(
        Guid UserId,
        Guid TargetUserId
    );

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