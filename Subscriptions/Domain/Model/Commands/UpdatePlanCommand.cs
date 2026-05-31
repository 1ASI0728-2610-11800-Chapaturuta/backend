namespace Frock_backend.Subscriptions.Domain.Model.Commands;

public record UpdatePlanCommand(
    int Id,
    decimal Price,
    string Benefits,
    int? DiscoveryQuota,
    bool IsActive
);
