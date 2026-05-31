using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Subscriptions.Domain;

public class SubscriptionAggregateTests
{
    private static Subscription CreateSubscription()
    {
        return new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: false);
    }

    [Fact]
    public void Activate_Sets_Status_Active_StartsAt_EndsAt_FkIdPayment()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var starts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ends = starts.AddMonths(1);
        const int paymentId = 555;

        // ACT
        subscription.Activate(starts, ends, paymentId);

        // ASSERT
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(starts, subscription.StartsAt);
        Assert.Equal(ends, subscription.EndsAt);
        Assert.Equal(paymentId, subscription.FkIdPayment);
    }

    [Fact]
    public void ActivateFree_Sets_Status_Active_Without_Payment()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var starts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ends = starts.AddMonths(1);

        // ACT
        subscription.ActivateFree(starts, ends);

        // ASSERT
        Assert.Equal(SubscriptionStatus.Active, subscription.Status);
        Assert.Equal(starts, subscription.StartsAt);
        Assert.Equal(ends, subscription.EndsAt);
        Assert.Null(subscription.FkIdPayment);
    }

    [Fact]
    public void Cancel_Sets_Status_Cancelled()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var starts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        subscription.ActivateFree(starts, starts.AddMonths(1));

        // ACT
        subscription.Cancel();

        // ASSERT
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public void ConsumeDiscoveryQuota_Increments_Counter()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var before = subscription.DiscoveryUsageInCycle;

        // ACT
        subscription.ConsumeDiscoveryQuota();
        subscription.ConsumeDiscoveryQuota();

        // ASSERT
        Assert.Equal(before + 2, subscription.DiscoveryUsageInCycle);
    }

    [Fact]
    public void IsActive_Returns_False_When_EndsAt_In_Past()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var starts = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ends = starts.AddMonths(1);
        subscription.ActivateFree(starts, ends);
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ACT
        var result = subscription.IsActive(now);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public void IsActive_Returns_True_When_Active_And_EndsAt_Future()
    {
        // ARRANGE
        var subscription = CreateSubscription();
        var starts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ends = starts.AddMonths(1);
        subscription.ActivateFree(starts, ends);
        var now = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // ACT
        var result = subscription.IsActive(now);

        // ASSERT
        Assert.True(result);
    }
}
