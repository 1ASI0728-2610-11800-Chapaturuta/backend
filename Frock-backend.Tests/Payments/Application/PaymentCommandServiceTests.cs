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

public class PaymentCommandServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IYapePaymentGateway> _yape = new();
    private readonly Mock<IPlinPaymentGateway> _plin = new();
    private readonly Mock<ICardPaymentGateway> _card = new();
    private readonly Mock<ICashPaymentHandler> _cash = new();

    private PaymentGatewayFactory BuildFactory() =>
        new PaymentGatewayFactory(_yape.Object, _plin.Object, _card.Object, _cash.Object);

    private PaymentCommandService BuildService() =>
        new PaymentCommandService(_paymentRepo.Object, BuildFactory(), _uow.Object);

    [Fact]
    public async Task CreatePayment_Persists_Pending_Then_Calls_Gateway_And_Updates_ExternalReference()
    {
        // ARRANGE
        Payment? captured = null;
        _paymentRepo
            .Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Callback<Payment>(p => captured = p)
            .Returns(Task.CompletedTask);

        _yape
            .Setup(g => g.InitiateAsync(It.IsAny<Payment>()))
            .ReturnsAsync(new GatewayResult(true, "EXT-XYZ", "ok"));

        _uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        var service = BuildService();
        var command = new CreatePaymentCommand(
            FkIdUser: 1,
            Amount: 50m,
            Currency: "PEN",
            Method: PaymentMethod.Yape,
            ReferenceType: "Reservation",
            ReferenceId: 1);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(captured);
        _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        _uow.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
        _yape.Verify(g => g.InitiateAsync(It.IsAny<Payment>()), Times.Once);
        Assert.Equal("EXT-XYZ", result!.ExternalReference);
        // The persisted payment was Pending at the time of AddAsync; we additionally check the
        // gateway-assigned external reference made it onto the aggregate.
        Assert.Equal("EXT-XYZ", captured!.ExternalReference);
        // Status remains Pending until a Confirm command is issued; CreatePayment only sets ExternalReference.
        Assert.Equal(PaymentStatus.Pending, captured.Status);
    }

    [Fact]
    public async Task ConfirmPayment_Loads_And_Calls_Aggregate_Confirm()
    {
        // ARRANGE
        var existing = new Payment(
            fkIdUser: 1,
            amount: new Money(50m, "PEN"),
            method: PaymentMethod.Yape,
            referenceType: "Reservation",
            referenceId: 1);

        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        var service = BuildService();
        var command = new ConfirmPaymentCommand(PaymentId: 1, ExternalReference: "EXT-XYZ");

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Completed, result!.Status);
        Assert.Equal("EXT-XYZ", result.ExternalReference);
        _paymentRepo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task FailPayment_Sets_Status_Failed()
    {
        // ARRANGE
        var existing = new Payment(
            fkIdUser: 1,
            amount: new Money(50m, "PEN"),
            method: PaymentMethod.Yape,
            referenceType: "Reservation",
            referenceId: 1);

        _paymentRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(existing);
        _uow.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        var service = BuildService();
        var command = new FailPaymentCommand(PaymentId: 1);

        // ACT
        var result = await service.Handle(command);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(PaymentStatus.Failed, result!.Status);
        _paymentRepo.Verify(r => r.Update(existing), Times.Once);
        _uow.Verify(u => u.CompleteAsync(), Times.AtLeastOnce);
    }
}
