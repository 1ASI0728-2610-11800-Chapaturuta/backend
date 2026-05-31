using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;

namespace Frock_backend.Subscriptions.Interfaces.REST.Transform;

public static class SubscribeToPlanCommandFromResourceAssembler
{
    public static SubscribeToPlanCommand ToCommandFromResource(SubscribeToPlanResource resource) =>
        new SubscribeToPlanCommand(
            resource.FkIdUser,
            resource.FkIdPlan,
            resource.AutoRenew,
            resource.PaymentMethod
        );
}
