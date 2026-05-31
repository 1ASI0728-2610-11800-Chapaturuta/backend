namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record SubscriptionResource(
    int Id,
    int FkIdUser,
    int FkIdPlan,
    string Status,
    DateTime StartsAt,
    DateTime EndsAt,
    bool AutoRenew,
    int? FkIdPayment,
    int DiscoveryUsageInCycle
);
