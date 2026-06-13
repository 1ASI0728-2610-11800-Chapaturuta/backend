using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.Subscriptions.Application.Internal.CommandServices;
using Frock_backend.Subscriptions.Domain.Model.Aggregates;
using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Repositories;
using Frock_backend.shared.Domain.Repositories;
using Moq;

namespace Frock_backend.Tests.Subscriptions.Application;

public class SubscriptionCommandServiceTests
{
    private readonly Mock<ISubscriptionRepository> _subscriptionRepository = new();
    private readonly Mock<IPlanRepository> _planRepository = new();
    private readonly Mock<IPaymentsContextFacade> _paymentsFacade = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private SubscriptionCommandService CreateService()
        => new(_subscriptionRepository.Object, _planRepository.Object, _paymentsFacade.Object, _unitOfWork.Object);

    private static Plan CreatePlan(PlanType planType, decimal price = 29.90m, BillingCycle cycle = BillingCycle.Monthly)
    {
        return new Plan(
            name: planType == PlanType.Free ? "Free Plan" : "Premium Plan",
            planType: planType,
            targetRole: TargetRole.Traveller,
            price: price,
            currency: "PEN",
            billingCycle: cycle,
            benefits: "Test benefits",
            discoveryQuota: planType == PlanType.Free ? 10 : (int?)null);
    }

    [Fact]
    public async Task SubscribeToFreePlan_Activates_Without_Payment()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Free, price: 0m);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        Subscription? persisted = null;
        _subscriptionRepository
            .Setup(s => s.AddAsync(It.IsAny<Subscription>()))
            .Callback<Subscription>(sub => persisted = sub)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var command = new SubscribeToPlanCommand(FkIdUser: 1, FkIdPlan: 10, AutoRenew: false, PaymentMethod: PaymentMethod.Cash);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(persisted);
        Assert.Equal(SubscriptionStatus.Active, persisted!.Status);
        Assert.Null(persisted.FkIdPayment);
        _paymentsFacade.Verify(
            f => f.RegisterPendingPaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SubscribeToPremiumPlan_Registers_Payment_And_Stays_Pending()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium, price: 29.90m);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);
        _subscriptionRepository
            .Setup(s => s.FindPendingByUserIdAsync(1))
            .ReturnsAsync(Array.Empty<Subscription>());

        _paymentsFacade
            .Setup(f => f.RegisterPendingPaymentAsync(
                1, 29.90m, PaymentMethod.Yape, "Subscription", It.IsAny<int>()))
            .ReturnsAsync(555);

        Subscription? persisted = null;
        _subscriptionRepository
            .Setup(s => s.AddAsync(It.IsAny<Subscription>()))
            .Callback<Subscription>(sub => persisted = sub)
            .Returns(Task.CompletedTask);

        var service = CreateService();
        var command = new SubscribeToPlanCommand(FkIdUser: 1, FkIdPlan: 10, AutoRenew: true, PaymentMethod: PaymentMethod.Yape);

        // ACT
        var result = await service.Handle(command);

        // ASSERT: payment registered, but subscription stays PendingPayment until the payment is confirmed.
        Assert.NotNull(result);
        Assert.NotNull(persisted);
        Assert.Equal(SubscriptionStatus.PendingPayment, result!.Status);
        Assert.Equal(555, result.FkIdPayment);
        _paymentsFacade.Verify(
            f => f.RegisterPendingPaymentAsync(1, 29.90m, PaymentMethod.Yape, "Subscription", It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateSubscription_Activates_A_Pending_Subscription()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium, price: 29.90m);
        var subscription = new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: true);
        subscription.AttachPendingPayment(paymentId: 555, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));

        _subscriptionRepository.Setup(s => s.FindByIdAsync(100)).ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var service = CreateService();

        // ACT
        var result = await service.Handle(new ActivateSubscriptionCommand(100));

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(SubscriptionStatus.Active, result!.Status);
        Assert.Equal(555, result.FkIdPayment);
        _subscriptionRepository.Verify(s => s.Update(subscription), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ActivateSubscription_Is_Idempotent_When_Already_Active()
    {
        // ARRANGE
        var subscription = new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: true);
        subscription.Activate(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), paymentId: 555);

        _subscriptionRepository.Setup(s => s.FindByIdAsync(100)).ReturnsAsync(subscription);

        var service = CreateService();

        // ACT
        var result = await service.Handle(new ActivateSubscriptionCommand(100));

        // ASSERT: no-op, plan never looked up, nothing persisted.
        Assert.NotNull(result);
        Assert.Equal(SubscriptionStatus.Active, result!.Status);
        _planRepository.Verify(p => p.FindByIdAsync(It.IsAny<int>()), Times.Never);
        _subscriptionRepository.Verify(s => s.Update(It.IsAny<Subscription>()), Times.Never);
    }

    [Fact]
    public async Task CancelSubscription_Sets_Status_Cancelled_And_Requests_Refund_When_Within_7_Days_And_Premium()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium, price: 29.90m);
        var subscription = new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: false);
        var startedAt = DateTime.UtcNow.AddDays(-3);
        subscription.Activate(startedAt, startedAt.AddMonths(1), paymentId: 555);

        _subscriptionRepository.Setup(s => s.FindByIdAsync(100)).ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);
        _paymentsFacade
            .Setup(f => f.RegisterRefundAsync(555, 29.90m, It.IsAny<string>()))
            .ReturnsAsync(1);

        var service = CreateService();

        // ACT
        var result = await service.Handle(new CancelSubscriptionCommand(100));

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(SubscriptionStatus.Cancelled, result!.Status);
        _paymentsFacade.Verify(f => f.RegisterRefundAsync(555, 29.90m, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CancelSubscription_Skips_Refund_When_After_7_Days()
    {
        // ARRANGE
        var plan = CreatePlan(PlanType.Premium, price: 29.90m);
        var subscription = new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: false);
        var startedAt = DateTime.UtcNow.AddDays(-30);
        subscription.Activate(startedAt, startedAt.AddMonths(2), paymentId: 555);

        _subscriptionRepository.Setup(s => s.FindByIdAsync(100)).ReturnsAsync(subscription);
        _planRepository.Setup(p => p.FindByIdAsync(10)).ReturnsAsync(plan);

        var service = CreateService();

        // ACT
        var result = await service.Handle(new CancelSubscriptionCommand(100));

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(SubscriptionStatus.Cancelled, result!.Status);
        _paymentsFacade.Verify(
            f => f.RegisterRefundAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsumeDiscoveryQuota_Increments_When_Active_Subscription_Exists()
    {
        // ARRANGE
        var subscription = new Subscription(fkIdUser: 1, fkIdPlan: 10, autoRenew: false);
        subscription.ActivateFree(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddMonths(1));
        var previous = subscription.DiscoveryUsageInCycle;

        _subscriptionRepository
            .Setup(s => s.FindActiveByUserIdAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(subscription);

        var service = CreateService();

        // ACT
        var result = await service.Handle(new ConsumeDiscoveryQuotaCommand(1));

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(previous + 1, result!.DiscoveryUsageInCycle);
        _subscriptionRepository.Verify(s => s.Update(subscription), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }
}
