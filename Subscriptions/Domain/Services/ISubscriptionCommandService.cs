using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Commands;

namespace Frock_backend.Subscriptions.Domain.Services;

public interface ISubscriptionCommandService
{
    Task<Subscription?> Handle(SubscribeToPlanCommand command);
    Task<Subscription?> Handle(ActivateSubscriptionCommand command);
    Task<Subscription?> Handle(CancelSubscriptionCommand command);
    Task<Subscription?> Handle(RenewSubscriptionCommand command);
    Task<Subscription?> Handle(ConsumeDiscoveryQuotaCommand command);
}
