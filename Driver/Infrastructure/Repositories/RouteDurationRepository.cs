using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Driver.Infrastructure.Repositories;

public class RouteDurationRepository(AppDbContext context) : BaseRepository<RouteDuration>(context), IRouteDurationRepository
{
    public async Task<RouteDuration?> FindByTariffAndRouteAsync(int tariffId, int routeId)
    {
        return await Context.Set<RouteDuration>()
            .FirstOrDefaultAsync(rd => rd.FkIdTariff == tariffId && rd.FkIdRoute == routeId);
    }
}
