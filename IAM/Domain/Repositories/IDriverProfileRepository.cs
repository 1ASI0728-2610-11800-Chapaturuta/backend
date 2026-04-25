using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.IAM.Domain.Repositories;

public interface IDriverProfileRepository : IBaseRepository<DriverProfile>
{
    Task<DriverProfile?> FindByUserIdAsync(int userId);
}
