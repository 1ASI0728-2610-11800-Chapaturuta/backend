using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Services.Gateways;

namespace Frock_backend.Payments.Infrastructure.ExternalServices.Gateways;

public class YapePaymentGateway : IYapePaymentGateway
{
    public Task<GatewayResult> InitiateAsync(Payment payment)
    {
        return Task.FromResult(new GatewayResult(true, $"STUB-{Guid.NewGuid()}", "stub"));
    }

    public Task<GatewayResult> ConfirmAsync(string externalRef)
    {
        return Task.FromResult(new GatewayResult(true, externalRef, "stub"));
    }

    public Task<GatewayResult> RefundAsync(Payment payment, decimal amount)
    {
        return Task.FromResult(new GatewayResult(true, $"STUB-{Guid.NewGuid()}", "stub"));
    }
}
