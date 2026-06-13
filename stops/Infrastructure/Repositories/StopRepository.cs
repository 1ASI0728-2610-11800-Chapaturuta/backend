using Frock_backend.shared.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;


using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace Frock_backend.stops.Infrastructure.Repositories
{
    public class StopRepository(AppDbContext context) : BaseRepository<Stop>(context), IStopRepository
    {
        public async Task<IEnumerable<Stop>> FindByFkIdDriverAsync(int fkIdDriver)
        {
            return await Context.Set<Stop>()
                .Where(f => f.FkIdDriver == fkIdDriver)
                .ToListAsync();
        }
        public async Task<int> CountByFkIdDriverAsync(int fkIdDriver)
        {
            return await Context.Set<Stop>()
                .CountAsync(f => f.FkIdDriver == fkIdDriver);
        }
        public async Task<IEnumerable<Stop>> FindByFkIdDistrictAsync(int fkIdDistrict)
        {
            return await Context.Set<Stop>()
                .Where(f => f.FkIdDistrict == fkIdDistrict)
                .ToListAsync();
        }
        public async Task<Stop?> FindByNameAndFkIdDistrictAsync(string name, int fkIdDistrict)
        {
            return await context.Set<Stop>()
                .FirstOrDefaultAsync(f => f.Name == name && f.FkIdDistrict == fkIdDistrict);
        }

        public async Task<Stop?> FindByNameAndFkIdDriverAsync(string name, int fkIdDriver)
        {
            return await context.Set<Stop>()
                .FirstOrDefaultAsync(f => f.Name == name && f.FkIdDriver == fkIdDriver);
        }
    }
}
