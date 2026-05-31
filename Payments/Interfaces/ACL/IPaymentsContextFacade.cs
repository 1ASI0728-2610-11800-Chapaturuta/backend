using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Payments.Interfaces.ACL;

public interface IPaymentsContextFacade
{
    Task<int> RegisterPendingPaymentAsync(int userId, decimal amount, PaymentMethod method, string referenceType, int referenceId);
    Task ConfirmPaymentAsync(int paymentId, string externalReference);
    Task<int> RegisterRefundAsync(int paymentId, decimal amount, string reason);
}
