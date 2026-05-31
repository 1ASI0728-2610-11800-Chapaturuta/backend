using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Subscriptions.Domain.Repositories;

public interface IPlanRepository : IBaseRepository<Plan>
{
    Task<IEnumerable<Plan>> FindActiveByTargetRoleAsync(TargetRole role);
}
