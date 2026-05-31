using Frock_backend.Payments.Application.Internal.CommandServices;
using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services.Gateways;
using Frock_backend.Payments.Infrastructure.Factories;
using Frock_backend.shared.Domain.Repositories;
using Moq;

namespace Frock_backend.Tests.Payments.Application;

public class RefundCommandServiceTests
{
    private readonly Mock<IRefundRepository> _refundRepo = new();
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IYapePaymentGateway> _yape = new();
    private readonly Mock<IPlinPaymentGateway> _plin = new();
    private readonly Mock<ICardPaymentGateway> _card = new();
    private readonly Mock<ICashPaymentHandler> _cash = new();

    private PaymentGatewayFactory BuildFactory() =>
        new PaymentGatewayFactory(_yape.Object, _plin.Object, _card.Object, _cash.Object);

    private RefundCommandService BuildService() =>
        new RefundCommandService(_refundRepo.Object, _paymentRepo.Object, BuildFactory(), _uow.Object);

    private static Payment NewCompletedPayment(decimal amount = 100m)
    {
        var payment = new Payment(
            fkIdUser: 1,
            amount: new Money(amount, "PEN"),
            method: PaymentMethod.Yape,
            referenceType: "Reservation",
            referenceId: 1);
        payment.Confirm("EXT-OK");
        return payment;
    }

    [Fact]
    public async Task CreateRefund_Throws_When_Payment_Not_Completed()
    {
        // ARRANGE
        var pending = new Payment(
            fkIdUser: 1,
            amount: new Money(100m, "PEN"),
            method: PaymentMethod.Yape,
            referenceType: "Reservation",
            referenceId: 1);
        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(pending);

        var service = BuildService();
        var command = new CreateRefundCommand(FkIdPayment: 1, Amount: 50m, Reason: "test");

        // ACT + ASSERT
        // The service wraps the InvalidOperationException in a generic Exception via try/catch in some
        // branches; however, the status guard runs BEFORE the try block, so the original
        // InvalidOperationException propagates here.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Handle(command));
    }

    [Fact]
    public async Task CreateRefund_Full_Sets_Payment_Status_Refunded()
    {
        // ARRANGE
        var payment = NewCompletedPayment(amount: 100m);
        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(payment);
        _refundRepo.Setup(r => r.AddAsync(It.IsAny<Refund>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _yape.Setup(g => g.RefundAsync(It.IsAny<Payment>(), It.IsAny<decimal>()))
             .ReturnsAsync(new GatewayResult(true, "REF-OK", "ok"));

        var service = BuildService();
        var command = new CreateRefundCommand(FkIdPayment: 1, Amount: 100m, Reason: "full refund");

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        _paymentRepo.Verify(r => r.Update(payment), Times.Once);
    }

    [Fact]
    public async Task CreateRefund_Partial_Sets_Payment_Status_PartiallyRefunded()
    {
        // ARRANGE
        var payment = NewCompletedPayment(amount: 100m);
        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(payment);
        _refundRepo.Setup(r => r.AddAsync(It.IsAny<Refund>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _yape.Setup(g => g.RefundAsync(It.IsAny<Payment>(), It.IsAny<decimal>()))
             .ReturnsAsync(new GatewayResult(true, "REF-OK", "ok"));

        var service = BuildService();
        var command = new CreateRefundCommand(FkIdPayment: 1, Amount: 30m, Reason: "partial refund");

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        _paymentRepo.Verify(r => r.Update(payment), Times.Once);
    }

    [Fact]
    public async Task CreateRefund_Throws_When_Amount_Exceeds_Payment()
    {
        // ARRANGE
        var payment = NewCompletedPayment(amount: 100m);
        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(payment);

        var service = BuildService();
        var command = new CreateRefundCommand(FkIdPayment: 1, Amount: 200m, Reason: "too much");

        // ACT + ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => service.Handle(command));
    }
}
