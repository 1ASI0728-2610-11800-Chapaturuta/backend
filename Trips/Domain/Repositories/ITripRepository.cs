using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Trips.Domain.Repositories;

public interface ITripRepository : IBaseRepository<Trip>
{
    Task<IEnumerable<Trip>> FindByUserIdAsync(int userId);
    Task<IEnumerable<Trip>> FindByDriverIdAsync(int driverId);
}
