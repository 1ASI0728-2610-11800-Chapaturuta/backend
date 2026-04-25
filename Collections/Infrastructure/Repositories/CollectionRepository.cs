using Frock_backend.Collections.Domain.Model.Aggregates;
using Frock_backend.Collections.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Collections.Infrastructure.Repositories;

public class CollectionRepository(AppDbContext context) : BaseRepository<Collection>(context), ICollectionRepository
{
    public async Task<IEnumerable<Collection>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Collection>().Where(c => c.FkIdUser == userId).Include(c => c.Items).OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Collection?> FindByIdWithItemsAsync(int collectionId)
    {
        return await Context.Set<Collection>().Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == collectionId);
    }
}
