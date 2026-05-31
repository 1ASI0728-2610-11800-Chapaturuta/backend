namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record PlanResource(
    int Id,
    string Name,
    string PlanType,
    string TargetRole,
    decimal Price,
    string Currency,
    string BillingCycle,
    string Benefits,
    int? DiscoveryQuota,
    bool IsActive
);
