using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Model.Queries;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.IAM.Domain.Services;

namespace Frock_backend.IAM.Application.Internal.QueryServices;

public class UserQueryService(IUserRepository userRepository, IDriverProfileRepository driverProfileRepository) : IUserQueryService
{
    public async Task<User?> Handle(GetUserByIdQuery query)
    {
        return await userRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery query)
    {
        return await userRepository.ListAsync();
    }

    public async Task<User?> Handle(GetUserByUsernameQuery query)
    {
        return await userRepository.FindByUsernameAsync(query.Username);
    }

    public async Task<User?> Handle(GetUserByEmailQuery query)
    {
        return await userRepository.FindByEmailAsync(query.Email);
    }

    public async Task<DriverProfile?> Handle(GetDriverProfileByUserIdQuery query)
    {
        return await driverProfileRepository.FindByUserIdAsync(query.UserId);
    }
}