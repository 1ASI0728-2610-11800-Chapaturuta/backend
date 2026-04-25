using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.IAM.Domain.Repositories;
public interface IUserRepository : IBaseRepository<User>
{
    /**
     * <summary>
     *     Find a user by id
     * </summary>
     * <param name="username">The username to search</param>
     * <returns>The user</returns>
     */
    Task<User?> FindByUsernameAsync(string username);

    Task<User?> FindByEmailAsync(string email);

    Task<bool> ExistsByEmail(string email);

    /**
     * <summary>
     *     Check if a user exists by username
     * </summary>
     * <param name="username">The username to search</param>
     * <returns>True if the user exists, false otherwise</returns>
     */
    bool ExistsByUsername(string username);
}