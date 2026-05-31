using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;

namespace Frock_backend.Payments.Domain.Services.Gateways;

public interface IPayUPaymentGateway : ICardPaymentGateway
{
    Task<GatewayResult> ChargeWithTokenAsync(Payment payment, PayUChargeInput input);
}
