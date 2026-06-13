namespace Frock_backend.Subscriptions.Interfaces.ACL;

public interface ISubscriptionsContextFacade
{
    Task<bool> HasActivePremiumPlanAsync(int userId);
    Task<int?> GetRemainingDiscoveryQuotaAsync(int userId);
    Task ConsumeDiscoveryQuotaAsync(int userId);

    /// <summary>
    ///     Activates a subscription awaiting payment once its backing payment has been confirmed.
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to activate.</param>
    Task ActivateSubscriptionAsync(int subscriptionId);
}
