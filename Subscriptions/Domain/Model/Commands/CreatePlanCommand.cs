using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Commands;

public record CreatePlanCommand(
    string Name,
    PlanType PlanType,
    TargetRole TargetRole,
    decimal Price,
    string Currency,
    BillingCycle BillingCycle,
    string Benefits,
    int? DiscoveryQuota
);
