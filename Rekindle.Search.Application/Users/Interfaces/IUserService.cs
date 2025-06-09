namespace Rekindle.Search.Application.Users.Interfaces;

public interface IUserService
{
    Task MergeUsersAsync(Guid groupId, Guid sourceUserId, Guid targetUserId,
        CancellationToken cancellationToken = default);
}