using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Commands;

namespace Frock_backend.IAM.Domain.Services;

public interface IUserCommandService
{
    Task<(User user, string token)> Handle(SignInCommand command);
    Task Handle(SignUpCommand command);
    Task<User?> Handle(UpdateUserProfileCommand command);
    Task<User?> Handle(UpdateUserRoleCommand command);
    Task<DriverProfile?> Handle(CreateDriverProfileCommand command);
}