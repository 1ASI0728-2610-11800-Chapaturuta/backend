using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;

namespace Frock_backend.Subscriptions.Interfaces.REST.Transform;

public static class CreatePlanCommandFromResourceAssembler
{
    public static CreatePlanCommand ToCommandFromResource(CreatePlanResource resource) =>
        new CreatePlanCommand(
            resource.Name,
            resource.PlanType,
            resource.TargetRole,
            resource.Price,
            string.IsNullOrWhiteSpace(resource.Currency) ? "PEN" : resource.Currency,
            resource.BillingCycle,
            resource.Benefits ?? string.Empty,
            resource.DiscoveryQuota
        );
}
