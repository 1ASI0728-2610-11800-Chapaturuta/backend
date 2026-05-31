using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Services.Gateways;
using Frock_backend.Payments.Infrastructure.Factories;
using Moq;

namespace Frock_backend.Tests.Payments.Infrastructure;

public class PaymentGatewayFactoryTests
{
    private readonly Mock<IYapePaymentGateway> _yape = new();
    private readonly Mock<IPlinPaymentGateway> _plin = new();
    private readonly Mock<ICardPaymentGateway> _card = new();
    private readonly Mock<ICashPaymentHandler> _cash = new();

    private PaymentGatewayFactory BuildFactory() =>
        new PaymentGatewayFactory(_yape.Object, _plin.Object, _card.Object, _cash.Object);

    [Fact]
    public void Resolve_Returns_YapeGateway_For_Yape()
    {
        // ARRANGE
        var factory = BuildFactory();

        // ACT
        var resolved = factory.Resolve(PaymentMethod.Yape);

        // ASSERT
        Assert.Same(_yape.Object, resolved);
    }

    [Fact]
    public void Resolve_Returns_PlinGateway_For_Plin()
    {
        // ARRANGE
        var factory = BuildFactory();

        // ACT
        var resolved = factory.Resolve(PaymentMethod.Plin);

        // ASSERT
        Assert.Same(_plin.Object, resolved);
    }

    [Fact]
    public void Resolve_Returns_CardGateway_For_Card()
    {
        // ARRANGE
        var factory = BuildFactory();

        // ACT
        var resolved = factory.Resolve(PaymentMethod.Card);

        // ASSERT
        Assert.Same(_card.Object, resolved);
    }

    [Fact]
    public void Resolve_Returns_CashHandler_For_Cash()
    {
        // ARRANGE
        var factory = BuildFactory();

        // ACT
        var resolved = factory.Resolve(PaymentMethod.Cash);

        // ASSERT
        Assert.Same(_cash.Object, resolved);
    }
}
