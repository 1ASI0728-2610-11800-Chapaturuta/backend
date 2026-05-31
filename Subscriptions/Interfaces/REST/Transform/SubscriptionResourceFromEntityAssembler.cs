using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;

namespace Frock_backend.Subscriptions.Interfaces.REST.Transform;

public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription entity) =>
        new SubscriptionResource(
            entity.Id,
            entity.FkIdUser,
            entity.FkIdPlan,
            entity.Status.ToString(),
            entity.StartsAt,
            entity.EndsAt,
            entity.AutoRenew,
            entity.FkIdPayment,
            entity.DiscoveryUsageInCycle
        );
}
