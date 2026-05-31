using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Queries;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;

namespace Frock_backend.Subscriptions.Application.Internal.QueryServices;

public class SubscriptionQueryService(ISubscriptionRepository subscriptionRepository) : ISubscriptionQueryService
{
    public async Task<Subscription?> Handle(GetActiveSubscriptionByUserIdQuery query)
    {
        return await subscriptionRepository.FindActiveByUserIdAsync(query.FkIdUser, DateTime.UtcNow);
    }

    public async Task<IEnumerable<Subscription>> Handle(GetSubscriptionHistoryByUserIdQuery query)
    {
        return await subscriptionRepository.FindByUserIdAsync(query.FkIdUser);
    }
}
