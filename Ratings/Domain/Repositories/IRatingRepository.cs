using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Ratings.Domain.Repositories;

public interface IRatingRepository : IBaseRepository<Rating>
{
    Task<IEnumerable<Rating>> FindByDriverIdAsync(int driverId);
    Task<IEnumerable<Rating>> FindByUserIdAsync(int userId);
}
