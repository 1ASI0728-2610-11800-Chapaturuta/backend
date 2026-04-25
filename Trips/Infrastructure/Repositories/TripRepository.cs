using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Trips.Infrastructure.Repositories;

public class TripRepository(AppDbContext context) : BaseRepository<Trip>(context), ITripRepository
{
    public async Task<IEnumerable<Trip>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Trip>().Where(t => t.FkIdUser == userId).OrderByDescending(t => t.StartTime).ToListAsync();
    }

    public async Task<IEnumerable<Trip>> FindByDriverIdAsync(int driverId)
    {
        return await Context.Set<Trip>().Where(t => t.FkIdDriver == driverId).OrderByDescending(t => t.StartTime).ToListAsync();
    }
}
