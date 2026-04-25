using Frock_backend.IAM.Domain.Model.Commands;
using Frock_backend.IAM.Domain.Model.Queries;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Frock_backend.IAM.Domain.Services;

namespace Frock_backend.IAM.Interfaces.ACL.Services;
public class IamContextFacade(IUserCommandService userCommandService, IUserQueryService userQueryService) : IIamContextFacade
{
    /**
     * <summary>
     *     Creates a new user with username, email, and password.
     * </summary>
     * <param name="username">The username for the new user.</param>
     * <param name="email">The email for the new user.</param>
     * <param name="password">The password for the new user.</param>
     * <returns>The ID of the newly created user.</returns>
     */
    public async Task<int> CreateUser(string username, string email, string password, Role role)
    {
        var signUpCommand = new SignUpCommand
        {
            Username = username,
            Email = email,
            Password = password,
            Role = role
        };

        await userCommandService.Handle(signUpCommand);

        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery);

        return result?.Id ?? 0;
    }

    /**
     * <summary>
     *     Fetch the user ID given a username.
     * </summary>
     * <param name="username">The username to search for.</param>
     * <returns>The ID of the user, or 0 if not found.</returns>
     */
    public async Task<int> FetchUserIdByUsername(string username)
    {
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery);
        return result?.Id ?? 0;
    }

    public async Task<int> FetchUserIdByEmail(string email)
    {
        var getUserByEmailQuery = new GetUserByEmailQuery(email);
        var result = await userQueryService.Handle(getUserByEmailQuery);
        return result?.Id ?? 0;
    }

    /**
     * <summary>
     *     Fetch the username given a user ID.
     * </summary>
     * <param name="userId">The ID of the user.</param>
     * <returns>The username, or empty string if not found.</returns>
     */
    public async Task<string> FetchUsernameByUserId(int userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery);
        return result?.Username ?? string.Empty;
    }

    public async Task<string> FetchEmailByUserId(int userId)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery);
        return result?.Email ?? string.Empty;
    }
}
