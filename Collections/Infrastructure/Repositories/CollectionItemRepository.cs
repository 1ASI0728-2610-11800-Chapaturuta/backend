using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Collections.Infrastructure.Repositories;

public class CollectionItemRepository(AppDbContext context) : BaseRepository<CollectionItem>(context), ICollectionItemRepository
{
    public async Task<CollectionItem?> FindByCollectionAndRouteAsync(int collectionId, int routeId)
    {
        return await Context.Set<CollectionItem>().FirstOrDefaultAsync(ci => ci.FkIdCollection == collectionId && ci.FkIdRoute == routeId);
    }

    public async Task<IEnumerable<CollectionItem>> FindByCollectionIdAsync(int collectionId)
    {
        return await Context.Set<CollectionItem>().Where(ci => ci.FkIdCollection == collectionId).ToListAsync();
    }
}
