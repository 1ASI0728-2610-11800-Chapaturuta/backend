using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Commands;

public record SubscribeToPlanCommand(
    int FkIdUser,
    int FkIdPlan,
    bool AutoRenew,
    PaymentMethod PaymentMethod
);
