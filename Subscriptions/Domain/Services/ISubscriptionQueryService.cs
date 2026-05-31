using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Queries;

namespace Frock_backend.Subscriptions.Domain.Services;

public interface ISubscriptionQueryService
{
    Task<Subscription?> Handle(GetActiveSubscriptionByUserIdQuery query);
    Task<IEnumerable<Subscription>> Handle(GetSubscriptionHistoryByUserIdQuery query);
}
