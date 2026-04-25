using Frock_backend.Ratings.Domain.Model.Aggregates;
using Frock_backend.Ratings.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Ratings.Infrastructure.Repositories;

public class RatingRepository(AppDbContext context) : BaseRepository<Rating>(context), IRatingRepository
{
    public async Task<IEnumerable<Rating>> FindByDriverIdAsync(int driverId)
    {
        return await Context.Set<Rating>().Where(r => r.FkIdDriver == driverId).OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Rating>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Rating>().Where(r => r.FkIdUser == userId).OrderByDescending(r => r.CreatedAt).ToListAsync();
    }
}
