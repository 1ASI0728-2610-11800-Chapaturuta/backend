using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Services.Gateways;

namespace Frock_backend.Payments.Infrastructure.Factories;

public class PaymentGatewayFactory
{
    private readonly IYapePaymentGateway _yape;
    private readonly IPlinPaymentGateway _plin;
    private readonly ICardPaymentGateway _card;
    private readonly ICashPaymentHandler _cash;

    public PaymentGatewayFactory(
        IYapePaymentGateway yape,
        IPlinPaymentGateway plin,
        ICardPaymentGateway card,
        ICashPaymentHandler cash)
    {
        _yape = yape;
        _plin = plin;
        _card = card;
        _cash = cash;
    }

    public IPaymentGateway Resolve(PaymentMethod method) => method switch
    {
        PaymentMethod.Yape => _yape,
        PaymentMethod.Plin => _plin,
        PaymentMethod.Card => _card,
        PaymentMethod.Cash => _cash,
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unsupported payment method")
    };
}
