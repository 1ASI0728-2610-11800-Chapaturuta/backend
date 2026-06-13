using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Subscriptions.Domain.Model.Aggregates;

public class Subscription
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public int FkIdPlan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool AutoRenew { get; set; }
    public int? FkIdPayment { get; set; }
    public int DiscoveryUsageInCycle { get; set; }

    protected Subscription()
    {
        Status = SubscriptionStatus.PendingPayment;
        StartsAt = DateTime.UtcNow;
        EndsAt = DateTime.UtcNow;
        DiscoveryUsageInCycle = 0;
    }

    public Subscription(int fkIdUser, int fkIdPlan, bool autoRenew)
    {
        FkIdUser = fkIdUser;
        FkIdPlan = fkIdPlan;
        AutoRenew = autoRenew;
        Status = SubscriptionStatus.PendingPayment;
        StartsAt = DateTime.UtcNow;
        EndsAt = DateTime.UtcNow;
        DiscoveryUsageInCycle = 0;
        FkIdPayment = null;
    }

    public void Activate(DateTime starts, DateTime ends, int paymentId)
    {
        if (ends <= starts)
            throw new InvalidOperationException("Subscription end date must be after start date");
        Status = SubscriptionStatus.Active;
        StartsAt = starts;
        EndsAt = ends;
        FkIdPayment = paymentId;
        DiscoveryUsageInCycle = 0;
    }

    /// <summary>
    ///     Links a pending payment to the subscription and sets its billing window without activating it.
    ///     The subscription stays in <see cref="SubscriptionStatus.PendingPayment"/> until the payment is confirmed.
    /// </summary>
    public void AttachPendingPayment(int paymentId, DateTime starts, DateTime ends)
    {
        if (ends <= starts)
            throw new InvalidOperationException("Subscription end date must be after start date");
        Status = SubscriptionStatus.PendingPayment;
        StartsAt = starts;
        EndsAt = ends;
        FkIdPayment = paymentId;
        DiscoveryUsageInCycle = 0;
    }

    /// <summary>
    ///     Activates a subscription that is awaiting payment, recomputing its billing window from confirmation time.
    ///     Idempotent: a subscription that is already active is left untouched.
    /// </summary>
    public void ActivatePending(DateTime starts, DateTime ends)
    {
        if (Status == SubscriptionStatus.Active) return;
        if (ends <= starts)
            throw new InvalidOperationException("Subscription end date must be after start date");
        Status = SubscriptionStatus.Active;
        StartsAt = starts;
        EndsAt = ends;
    }

    public void ActivateFree(DateTime starts, DateTime ends)
    {
        if (ends <= starts)
            throw new InvalidOperationException("Subscription end date must be after start date");
        Status = SubscriptionStatus.Active;
        StartsAt = starts;
        EndsAt = ends;
        FkIdPayment = null;
        DiscoveryUsageInCycle = 0;
    }

    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Subscription is already cancelled");
        Status = SubscriptionStatus.Cancelled;
    }

    public void Renew(DateTime newEndsAt, int paymentId)
    {
        if (newEndsAt <= EndsAt)
            throw new InvalidOperationException("Renewal end date must be after current end date");
        Status = SubscriptionStatus.Active;
        EndsAt = newEndsAt;
        FkIdPayment = paymentId;
        DiscoveryUsageInCycle = 0;
    }

    public void MarkExpired()
    {
        Status = SubscriptionStatus.Expired;
    }

    public void ConsumeDiscoveryQuota()
    {
        DiscoveryUsageInCycle += 1;
    }

    public bool IsActive(DateTime now) =>
        Status == SubscriptionStatus.Active && EndsAt > now;
}
