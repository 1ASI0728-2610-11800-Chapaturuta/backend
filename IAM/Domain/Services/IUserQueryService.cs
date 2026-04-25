using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Queries;

namespace Frock_backend.IAM.Domain.Services;

public interface IUserQueryService
{
    /**
     * <summary>
     *     Handle get user by id query
     * </summary>
     * <param name="query">The get user by id query</param>
     * <returns>The user if found, null otherwise</returns>
     */
    Task<User?> Handle(GetUserByIdQuery query);

    /**
     * <summary>
     *     Handle get all users query
     * </summary>
     * <param name="query">The get all users query</param>
     * <returns>The list of users</returns>
     */
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query);
    
    /**
     * <summary>
     *     Handle get user by username query
     * </summary>
     * <param name="query">The get user by username query</param>
     * <returns>The user if found, null otherwise</returns>
     */
    Task<User?> Handle(GetUserByUsernameQuery query);

    Task<User?> Handle(GetUserByEmailQuery query);
    Task<DriverProfile?> Handle(GetDriverProfileByUserIdQuery query);
}