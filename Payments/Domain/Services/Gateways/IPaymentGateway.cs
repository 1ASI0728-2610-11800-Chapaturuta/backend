using Frock_backend.Payments.Domain.Model.Aggregates;

namespace Frock_backend.Payments.Domain.Services.Gateways;

public record GatewayResult(bool Success, string? ExternalReference, string? Message);

public interface IPaymentGateway
{
    Task<GatewayResult> InitiateAsync(Payment payment);
    Task<GatewayResult> ConfirmAsync(string externalRef);
    Task<GatewayResult> RefundAsync(Payment payment, decimal amount);
}
