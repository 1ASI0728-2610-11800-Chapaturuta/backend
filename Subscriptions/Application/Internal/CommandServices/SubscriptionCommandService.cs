using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Subscriptions.Application.Internal.CommandServices;

public class SubscriptionCommandService(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IPaymentsContextFacade paymentsContextFacade,
    IUnitOfWork unitOfWork) : ISubscriptionCommandService
{
    private const string SubscriptionReferenceType = "Subscription";
    private const int RefundWindowDays = 7;

    public async Task<Subscription?> Handle(SubscribeToPlanCommand command)
    {
        var plan = await planRepository.FindByIdAsync(command.FkIdPlan);
        if (plan == null) throw new InvalidOperationException($"Plan {command.FkIdPlan} not found");
        if (!plan.IsActive) throw new InvalidOperationException($"Plan {command.FkIdPlan} is not active");

        var now = DateTime.UtcNow;
        var endsAt = ComputeEndDate(now, plan.BillingCycle);

        // Impide pagar dos veces por Premium: si el usuario ya tiene una suscripción Premium
        // activa y vigente, no puede contratar otra (debe cancelarla o esperar a que venza).
        if (plan.PlanType == PlanType.Premium)
        {
            var existingActive = await subscriptionRepository.FindActiveByUserIdAsync(command.FkIdUser, now);
            if (existingActive != null && existingActive.IsActive(now))
            {
                var existingPlan = await planRepository.FindByIdAsync(existingActive.FkIdPlan);
                if (existingPlan?.PlanType == PlanType.Premium)
                    throw new InvalidOperationException(
                        "Ya tienes una suscripción Premium activa. Cancélala o espera a que venza antes de contratar otra.");
            }
        }

        var subscription = new Subscription(command.FkIdUser, command.FkIdPlan, command.AutoRenew);

        try
        {
            if (plan.PlanType == PlanType.Free)
            {
                subscription.ActivateFree(now, endsAt);
                await subscriptionRepository.AddAsync(subscription);
                await unitOfWork.CompleteAsync();
                return subscription;
            }

            // Drop any abandoned PendingPayment attempts so the history doesn't pile up.
            await CancelStalePendingSubscriptionsAsync(command.FkIdUser);

            // Premium flow: persist as PendingPayment first to obtain an id, then register payment.
            // The subscription stays PendingPayment and is only activated once the payment is confirmed.
            await subscriptionRepository.AddAsync(subscription);
            await unitOfWork.CompleteAsync();

            var paymentId = await paymentsContextFacade.RegisterPendingPaymentAsync(
                command.FkIdUser,
                plan.Price,
                command.PaymentMethod,
                SubscriptionReferenceType,
                subscription.Id);

            if (paymentId == 0)
                throw new InvalidOperationException("Could not register payment for subscription");

            subscription.AttachPendingPayment(paymentId, now, endsAt);
            subscriptionRepository.Update(subscription);
            await unitOfWork.CompleteAsync();

            return subscription;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while subscribing to plan: {e.Message}");
        }
    }

    public async Task<Subscription?> Handle(ActivateSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId);
        if (subscription == null) return null;

        // Idempotent: PayU may retry its webhook and the demo confirm may be hit twice.
        if (subscription.Status == SubscriptionStatus.Active) return subscription;

        var plan = await planRepository.FindByIdAsync(subscription.FkIdPlan);
        if (plan == null) throw new InvalidOperationException($"Plan {subscription.FkIdPlan} not found");

        var now = DateTime.UtcNow;
        var endsAt = ComputeEndDate(now, plan.BillingCycle);

        try
        {
            subscription.ActivatePending(now, endsAt);
            subscriptionRepository.Update(subscription);
            await unitOfWork.CompleteAsync();
            return subscription;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while activating subscription: {e.Message}");
        }
    }

    public async Task<Subscription?> Handle(CancelSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId);
        if (subscription == null) return null;

        try
        {
            var plan = await planRepository.FindByIdAsync(subscription.FkIdPlan);
            var wasPremiumPaid = plan?.PlanType == PlanType.Premium
                                 && subscription.FkIdPayment.HasValue
                                 && subscription.FkIdPayment.Value > 0;
            var withinRefundWindow = subscription.StartsAt != default
                                     && (DateTime.UtcNow - subscription.StartsAt).TotalDays <= RefundWindowDays;

            subscription.Cancel();
            subscriptionRepository.Update(subscription);
            await unitOfWork.CompleteAsync();

            if (wasPremiumPaid && withinRefundWindow && plan != null)
            {
                await paymentsContextFacade.RegisterRefundAsync(
                    subscription.FkIdPayment!.Value,
                    plan.Price,
                    "Subscription cancelled within refund window");
            }

            return subscription;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while cancelling subscription: {e.Message}");
        }
    }

    public async Task<Subscription?> Handle(RenewSubscriptionCommand command)
    {
        var subscription = await subscriptionRepository.FindByIdAsync(command.SubscriptionId);
        if (subscription == null) return null;

        var plan = await planRepository.FindByIdAsync(subscription.FkIdPlan);
        if (plan == null) throw new InvalidOperationException($"Plan {subscription.FkIdPlan} not found");
        if (!plan.IsActive) throw new InvalidOperationException($"Plan {plan.Id} is not active");

        var renewFrom = subscription.EndsAt > DateTime.UtcNow ? subscription.EndsAt : DateTime.UtcNow;
        var newEndsAt = ComputeEndDate(renewFrom, plan.BillingCycle);

        try
        {
            if (plan.PlanType == PlanType.Free)
            {
                // Free plans renew without payment.
                subscription.Renew(newEndsAt, 0);
                subscription.FkIdPayment = null;
                subscriptionRepository.Update(subscription);
                await unitOfWork.CompleteAsync();
                return subscription;
            }

            var paymentId = await paymentsContextFacade.RegisterPendingPaymentAsync(
                subscription.FkIdUser,
                plan.Price,
                command.PaymentMethod,
                SubscriptionReferenceType,
                subscription.Id);

            if (paymentId == 0)
                throw new InvalidOperationException("Could not register payment for renewal");

            subscription.Renew(newEndsAt, paymentId);
            subscriptionRepository.Update(subscription);
            await unitOfWork.CompleteAsync();
            return subscription;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while renewing subscription: {e.Message}");
        }
    }

    public async Task<Subscription?> Handle(ConsumeDiscoveryQuotaCommand command)
    {
        var now = DateTime.UtcNow;
        var subscription = await subscriptionRepository.FindActiveByUserIdAsync(command.FkIdUser, now);
        if (subscription == null) return null;

        try
        {
            subscription.ConsumeDiscoveryQuota();
            subscriptionRepository.Update(subscription);
            await unitOfWork.CompleteAsync();
            return subscription;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while consuming discovery quota: {e.Message}");
        }
    }

    private async Task CancelStalePendingSubscriptionsAsync(int userId)
    {
        var pending = await subscriptionRepository.FindPendingByUserIdAsync(userId);
        foreach (var stale in pending)
        {
            stale.Cancel();
            subscriptionRepository.Update(stale);
        }
    }

    private static DateTime ComputeEndDate(DateTime from, BillingCycle cycle) => cycle switch
    {
        BillingCycle.Monthly => from.AddMonths(1),
        BillingCycle.Yearly => from.AddYears(1),
        _ => from.AddMonths(1)
    };
}
