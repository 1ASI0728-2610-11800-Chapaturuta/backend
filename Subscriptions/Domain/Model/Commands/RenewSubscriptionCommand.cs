using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Commands;

public record RenewSubscriptionCommand(
    int SubscriptionId,
    PaymentMethod PaymentMethod
);
