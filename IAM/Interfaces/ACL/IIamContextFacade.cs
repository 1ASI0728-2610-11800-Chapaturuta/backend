using Frock_backend.IAM.Domain.Model.ValueObjects;

namespace Frock_backend.IAM.Interfaces.ACL;

public interface IIamContextFacade
{
    Task<int> CreateUser(string username, string email, string password, Role role);
    Task<int> FetchUserIdByUsername(string username);

    Task<int> FetchUserIdByEmail(string email);

    Task<string> FetchUsernameByUserId(int userId);

    Task<string> FetchEmailByUserId(int userId);

    Task<string?> FetchUserRoleByIdAsync(int userId);

    /// <summary>
    ///     Bulk-resolves usernames keyed by user identifier, avoiding N+1 lookups.
    ///     Only existing users are present in the result.
    /// </summary>
    Task<IReadOnlyDictionary<int, string>> FetchUsernamesByUserIdsAsync(IEnumerable<int> userIds);
}
