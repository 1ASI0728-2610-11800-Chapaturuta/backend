using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;

namespace Frock_backend.Subscriptions.Interfaces.REST.Transform;

public static class PlanResourceFromEntityAssembler
{
    public static PlanResource ToResourceFromEntity(Plan entity) =>
        new PlanResource(
            entity.Id,
            entity.Name,
            entity.PlanType.ToString(),
            entity.TargetRole.ToString(),
            entity.Price,
            entity.Currency,
            entity.BillingCycle.ToString(),
            entity.Benefits,
            entity.DiscoveryQuota,
            entity.IsActive
        );
}
