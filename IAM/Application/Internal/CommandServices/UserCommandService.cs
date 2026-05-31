using Frock_backend.IAM.Application.Internal.OutboundServices;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Commands;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.IAM.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.IAM.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork)
    : IUserCommandService
{
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByEmailAsync(command.Email);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    public async Task Handle(SignUpCommand command)
    {
        if (await userRepository.ExistsByEmail(command.Email))
            throw new Exception($"Email '{command.Email}' is already registered");

        var hashedPassword = hashingService.HashPassword(command.Password);
        var user = new User(command.Email, command.Username, hashedPassword, command.Role);

        try
        {
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating user: {e.Message}");
        }
    }

    public async Task<User?> Handle(UpdateUserProfileCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.Id);
        if (user == null) return null;

        user.UpdateUsername(command.Username);
        user.UpdateEmail(command.Email);

        try
        {
            userRepository.Update(user);
            await unitOfWork.CompleteAsync();
            return user;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while updating user profile: {e.Message}");
        }
    }

    public async Task<User?> Handle(UpdateUserRoleCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.Id);
        if (user == null) return null;

        user.UpdateRole(command.Role);

        try
        {
            userRepository.Update(user);
            await unitOfWork.CompleteAsync();
            return user;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while updating user role: {e.Message}");
        }
    }

}
