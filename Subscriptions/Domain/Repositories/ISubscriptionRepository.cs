using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Subscriptions.Domain.Repositories;

public interface ISubscriptionRepository : IBaseRepository<Subscription>
{
    Task<Subscription?> FindActiveByUserIdAsync(int userId, DateTime now);
    Task<IEnumerable<Subscription>> FindByUserIdAsync(int userId);
}
