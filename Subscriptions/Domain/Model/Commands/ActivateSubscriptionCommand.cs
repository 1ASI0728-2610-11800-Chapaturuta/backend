namespace Frock_backend.Subscriptions.Domain.Model.Commands;

/// <summary>
///     Activates a subscription that is awaiting payment, once its payment has been confirmed.
/// </summary>
public record ActivateSubscriptionCommand(int SubscriptionId);
