using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;

namespace Frock_backend.Subscriptions.Interfaces.ACL.Services;

public class SubscriptionsContextFacade(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    ISubscriptionCommandService subscriptionCommandService) : ISubscriptionsContextFacade
{
    /**
     * <summary>
     *     Indicates whether the user currently holds an active Premium subscription.
     * </summary>
     * <param name="userId">The ID of the user being checked.</param>
     * <returns>True when the user has an Active subscription tied to a Premium plan and not expired.</returns>
     */
    public async Task<bool> HasActivePremiumPlanAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var subscription = await subscriptionRepository.FindActiveByUserIdAsync(userId, now);
        if (subscription == null) return false;
        if (!subscription.IsActive(now)) return false;

        var plan = await planRepository.FindByIdAsync(subscription.FkIdPlan);
        return plan != null && plan.PlanType == PlanType.Premium;
    }

    /**
     * <summary>
     *     Returns the remaining Discovery quota for the user in the current billing cycle.
     * </summary>
     * <param name="userId">The ID of the user being checked.</param>
     * <returns>
     *     0 when no active subscription exists; null when the active plan grants unlimited Discovery (Premium);
     *     otherwise the remaining quota (plan quota minus usage in cycle, never below 0).
     * </returns>
     */
    public async Task<int?> GetRemainingDiscoveryQuotaAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var subscription = await subscriptionRepository.FindActiveByUserIdAsync(userId, now);
        if (subscription == null) return 0;

        var plan = await planRepository.FindByIdAsync(subscription.FkIdPlan);
        if (plan == null) return 0;

        if (plan.PlanType == PlanType.Premium) return null;

        var quota = plan.DiscoveryQuota ?? 0;
        var remaining = quota - subscription.DiscoveryUsageInCycle;
        return remaining < 0 ? 0 : remaining;
    }

    /**
     * <summary>
     *     Records the consumption of one Discovery query against the user's active subscription cycle.
     * </summary>
     * <param name="userId">The ID of the user consuming the quota.</param>
     */
    public async Task ConsumeDiscoveryQuotaAsync(int userId)
    {
        await subscriptionCommandService.Handle(new ConsumeDiscoveryQuotaCommand(userId));
    }

    /**
     * <summary>
     *     Activates a subscription awaiting payment once its backing payment has been confirmed.
     * </summary>
     * <param name="subscriptionId">The ID of the subscription to activate.</param>
     */
    public async Task ActivateSubscriptionAsync(int subscriptionId)
    {
        await subscriptionCommandService.Handle(new ActivateSubscriptionCommand(subscriptionId));
    }
}
