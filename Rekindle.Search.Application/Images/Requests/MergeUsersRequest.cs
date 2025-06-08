namespace Rekindle.Search.Application.Images.Requests;

public record MergeUsersRequest(
    string GroupId,
    IEnumerable<string> SourcePersonIds,
    string TargetPersonId
);