using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Subscriptions.Infrastructure.Repositories;

public class SubscriptionRepository(AppDbContext context) : BaseRepository<Subscription>(context), ISubscriptionRepository
{
    public async Task<Subscription?> FindActiveByUserIdAsync(int userId, DateTime now)
    {
        return await Context.Set<Subscription>()
            .Where(s => s.FkIdUser == userId
                        && s.Status == SubscriptionStatus.Active
                        && s.EndsAt > now)
            .OrderByDescending(s => s.EndsAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Subscription>> FindByUserIdAsync(int userId)
    {
        return await Context.Set<Subscription>()
            .Where(s => s.FkIdUser == userId)
            .OrderByDescending(s => s.StartsAt)
            .ToListAsync();
    }
}
