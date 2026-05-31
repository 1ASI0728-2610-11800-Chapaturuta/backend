using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Services.Gateways;
using Frock_backend.Payments.Infrastructure.ExternalServices.Gateways;

namespace Frock_backend.Tests.Payments.Infrastructure;

public class GatewayStubTests
{
    private static Payment NewPayment(PaymentMethod method = PaymentMethod.Yape)
    {
        return new Payment(
            fkIdUser: 1,
            amount: new Money(25m, "PEN"),
            method: method,
            referenceType: "Reservation",
            referenceId: 5);
    }

    [Fact]
    public async Task YapePaymentGateway_Stub_Returns_Success_With_External_Ref()
    {
        // ARRANGE
        IPaymentGateway gateway = new YapePaymentGateway();
        var payment = NewPayment(PaymentMethod.Yape);

        // ACT
        var result = await gateway.InitiateAsync(payment);

        // ASSERT
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ExternalReference));
        Assert.StartsWith("STUB", result.ExternalReference);
    }

    [Fact]
    public async Task PlinPaymentGateway_Stub_Returns_Success_With_External_Ref()
    {
        // ARRANGE
        IPaymentGateway gateway = new PlinPaymentGateway();
        var payment = NewPayment(PaymentMethod.Plin);

        // ACT
        var result = await gateway.InitiateAsync(payment);

        // ASSERT
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ExternalReference));
        Assert.StartsWith("STUB", result.ExternalReference);
    }

    [Fact]
    public async Task CashPaymentHandler_Stub_Returns_Success_With_External_Ref()
    {
        // ARRANGE
        IPaymentGateway gateway = new CashPaymentHandler();
        var payment = NewPayment(PaymentMethod.Cash);

        // ACT
        var result = await gateway.InitiateAsync(payment);

        // ASSERT
        Assert.True(result.Success);
        Assert.False(string.IsNullOrEmpty(result.ExternalReference));
        Assert.StartsWith("STUB", result.ExternalReference);
    }
}
