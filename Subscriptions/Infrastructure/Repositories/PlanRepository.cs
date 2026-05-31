using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Subscriptions.Infrastructure.Repositories;

public class PlanRepository(AppDbContext context) : BaseRepository<Plan>(context), IPlanRepository
{
    public async Task<IEnumerable<Plan>> FindActiveByTargetRoleAsync(TargetRole role)
    {
        return await Context.Set<Plan>()
            .Where(p => p.IsActive && (p.TargetRole == role || p.TargetRole == TargetRole.Both))
            .OrderBy(p => p.Price)
            .ToListAsync();
    }
}
