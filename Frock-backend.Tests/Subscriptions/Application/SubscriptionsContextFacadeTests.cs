using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.Subscriptions.Interfaces.ACL.Services;
using Moq;

namespace Frock_backend.Tests.Subscriptions.Application;

public class SubscriptionsContextFacadeTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepository = new();
    private readonly Mock<IPlanRepository> _planRepository = new();
    private readonly Mock<ISubscriptionCommandService> _subscriptionCommandService = new();

    private SubscriptionsContextFacade CreateFacade()
        => new(_subscriptionRepository.Object, _planRepository.Object, _subscriptionCommandService.Object);

    private static Plan CreatePlan(PlanType planType, int? discoveryQuota = null)
    {
        return new Plan(
            name: planType == PlanType.Free ? "Free Plan" : "Premium Plan",
            planType: planType,
            targetRole: TargetRole.Traveller,
            price: planType == PlanType.Premium ? 29.90m : 0m,
            currency: "PEN",
            billingCycle: BillingCycle.Monthly,
            benefits: "Test",
            discoveryQuota: discoveryQuota);
    }

    private static Subscription CreateActiveSubscription(int fkIdPlan, int discoveryUsage = 0)
    {
        var sub = new Subscription(fkIdUser: 1, fkIdPlan: fkIdPlan, autoRenew: false);
        sub.ActivateFree(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddMonths(1));
        for (var i = 0; i < discoveryUsage; i++) sub.ConsumeDiscoveryQuota();
        return sub;
    }

    [Fact]
    public async Task HasActivePremiumPlan_Returns_True_When_Active_Premium_Sub_Exists()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium);
        var subscription = CreateActiveSubscription(fkIdPlan: 10);
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var facade = CreateFacade();

        // ACT
        var result = await facade.HasActivePremiumPlanAsync(1);

        // ASSERT
        Assert.True(result);
    }

    [Fact]
    public async Task HasActivePremiumPlan_Returns_False_When_No_Subscription()
    {
        // ARRANGE
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync((Subscription?)null);

        var facade = CreateFacade();

        // ACT
        var result = await facade.HasActivePremiumPlanAsync(1);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public async Task HasActivePremiumPlan_Returns_False_When_Sub_Plan_Is_Free()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Free, discoveryQuota: 10);
        var subscription = CreateActiveSubscription(fkIdPlan: 10);
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var facade = CreateFacade();

        // ACT
        var result = await facade.HasActivePremiumPlanAsync(1);

        // ASSERT
        Assert.False(result);
    }

    [Fact]
    public async Task GetRemainingDiscoveryQuota_Returns_Null_For_Premium()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium);
        var subscription = CreateActiveSubscription(fkIdPlan: 10);
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var facade = CreateFacade();

        // ACT
        var result = await facade.GetRemainingDiscoveryQuotaAsync(1);

        // ASSERT
        Assert.Null(result);
    }

    [Fact]
    public async Task GetRemainingDiscoveryQuota_Returns_Positive_For_Free_Under_Limit()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Free, discoveryQuota: 10);
        var subscription = CreateActiveSubscription(fkIdPlan: 10, discoveryUsage: 3);
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var facade = CreateFacade();

        // ACT
        var result = await facade.GetRemainingDiscoveryQuotaAsync(1);

        // ASSERT
        Assert.Equal(7, result);
    }

    [Fact]
    public async Task GetRemainingDiscoveryQuota_Returns_Zero_When_Quota_Exhausted()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Free, discoveryQuota: 10);
        var subscription = CreateActiveSubscription(fkIdPlan: 10, discoveryUsage: 10);
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var facade = CreateFacade();

        // ACT
        var result = await facade.GetRemainingDiscoveryQuotaAsync(1);

        // ASSERT
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetRemainingDiscoveryQuota_Returns_Zero_When_No_Active_Sub()
    {
        // ARRANGE
        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync((Subscription?)null);

        var facade = CreateFacade();

        // ACT
        var result = await facade.GetRemainingDiscoveryQuotaAsync(1);

        // ASSERT
        Assert.Equal(0, result);
    }
}
