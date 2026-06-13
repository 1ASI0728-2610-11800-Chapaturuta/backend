using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Queries;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Subscriptions.Application.Internal.QueryServices;

public class SubscriptionQueryService(
    ISubscriptionRepository subscriptionRepository,
    IUnitOfWork unitOfWork) : ISubscriptionQueryService
{
    public async Task<Subscription?> Handle(GetActiveSubscriptionByUserIdQuery query)
    {
        await ExpireOverdueAsync(query.FkIdUser);
        return await subscriptionRepository.FindActiveByUserIdAsync(query.FkIdUser, DateTime.UtcNow);
    }

    public async Task<IEnumerable<Subscription>> Handle(GetSubscriptionHistoryByUserIdQuery query)
    {
        await ExpireOverdueAsync(query.FkIdUser);
        return await subscriptionRepository.FindByUserIdAsync(query.FkIdUser);
    }

    // Lazy expiry: a Premium subscription is paid for one billing cycle (EndsAt). There is no
    // background job, so when its end date has passed we flip its status to Expired on read,
    // keeping the persisted status faithful for history and badges.
    private async Task ExpireOverdueAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var subscriptions = await subscriptionRepository.FindByUserIdAsync(userId);
        var changed = false;
        foreach (var subscription in subscriptions)
        {
            if (subscription.Status == SubscriptionStatus.Active && subscription.EndsAt <= now)
            {
                subscription.MarkExpired();
                subscriptionRepository.Update(subscription);
                changed = true;
            }
        }
        if (changed) await unitOfWork.CompleteAsync();
    }
}
