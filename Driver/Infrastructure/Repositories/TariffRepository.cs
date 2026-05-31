using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Driver.Infrastructure.Repositories;

public class TariffRepository(AppDbContext context) : BaseRepository<Tariff>(context), ITariffRepository
{
    public async Task<Tariff?> FindActiveByDriverIdAsync(int driverId)
    {
        return await Context.Set<Tariff>()
            .Where(t => t.FkIdDriver == driverId && t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
