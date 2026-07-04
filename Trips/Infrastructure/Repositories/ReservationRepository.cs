using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Trips.Infrastructure.Repositories;

public class ReservationRepository(AppDbContext context) : BaseRepository<Reservation>(context), IReservationRepository
{
    public async Task<IEnumerable<Reservation>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Reservation>()
            .Where(r => r.FkIdUser == userId)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> FindByTripIdAsync(int tripId)
    {
        return await Context.Set<Reservation>()
            .Where(r => r.FkIdTrip == tripId)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> FindByDriverIdAsync(int driverId)
    {
        return await Context.Set<Reservation>()
            .Join(
                Context.Set<Trip>(),
                r => r.FkIdTrip,
                t => t.Id,
                (r, t) => new { Reservation = r, Trip = t })
            .Where(x => x.Trip.FkIdDriver == driverId)
            .OrderByDescending(x => x.Reservation.ReservedAt)
            .Select(x => x.Reservation)
            .ToListAsync();
    }

    public async Task<List<Reservation>> FindByTripIdsAsync(IEnumerable<int> tripIds)
    {
        var idSet = tripIds.ToHashSet();
        if (idSet.Count == 0) return new List<Reservation>();

        return await Context.Set<Reservation>()
            .Where(r => idSet.Contains(r.FkIdTrip))
            .ToListAsync();
    }
}
