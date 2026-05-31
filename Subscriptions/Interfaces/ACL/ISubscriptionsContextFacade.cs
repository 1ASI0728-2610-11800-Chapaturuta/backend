namespace Frock_backend.Subscriptions.Interfaces.ACL;

public interface ISubscriptionsContextFacade
{
    Task<bool> HasActivePremiumPlanAsync(int userId);
    Task<int?> GetRemainingDiscoveryQuotaAsync(int userId);
    Task ConsumeDiscoveryQuotaAsync(int userId);
}
