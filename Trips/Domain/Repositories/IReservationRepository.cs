using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Trips.Domain.Repositories;

public interface IReservationRepository : IBaseRepository<Reservation>
{
    Task<IEnumerable<Reservation>> FindByUserIdAsync(int userId);
    Task<IEnumerable<Reservation>> FindByTripIdAsync(int tripId);
    Task<IEnumerable<Reservation>> FindByDriverIdAsync(int driverId);
    Task<List<Reservation>> FindByTripIdsAsync(IEnumerable<int> tripIds);
}
