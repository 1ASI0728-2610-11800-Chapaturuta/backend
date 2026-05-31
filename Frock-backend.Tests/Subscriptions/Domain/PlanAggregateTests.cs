using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Subscriptions.Domain;

public class PlanAggregateTests
{
    private static Plan CreatePlan(PlanType planType = PlanType.Premium, decimal price = 29.90m)
    {
        return new Plan(
            name: "Test Plan",
            planType: planType,
            targetRole: TargetRole.Traveller,
            price: price,
            currency: "PEN",
            billingCycle: BillingCycle.Monthly,
            benefits: "Test benefits",
            discoveryQuota: 10);
    }

    [Fact]
    public void Activate_Sets_IsActive_True()
    {
        // ARRANGE
        var plan = CreatePlan();
        plan.Deactivate();

        // ACT
        plan.Activate();

        // ASSERT
        Assert.True(plan.IsActive);
    }

    [Fact]
    public void Deactivate_Sets_IsActive_False()
    {
        // ARRANGE
        var plan = CreatePlan();

        // ACT
        plan.Deactivate();

        // ASSERT
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void UpdatePrice_Throws_When_Negative()
    {
        // ARRANGE
        var plan = CreatePlan();

        // ACT / ASSERT
        Assert.Throws<ArgumentException>(() => plan.UpdatePrice(-0.01m));
    }

    [Fact]
    public void UpdatePrice_Sets_New_Price_When_Positive_Or_Zero()
    {
        // ARRANGE
        var plan = CreatePlan(price: 10m);

        // ACT
        plan.UpdatePrice(0m);

        // ASSERT
        Assert.Equal(0m, plan.Price);

        // ACT (positive)
        plan.UpdatePrice(49.99m);

        // ASSERT
        Assert.Equal(49.99m, plan.Price);
    }
}
