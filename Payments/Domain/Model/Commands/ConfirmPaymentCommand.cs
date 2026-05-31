namespace Frock_backend.Payments.Domain.Model.Commands;

public record ConfirmPaymentCommand(int PaymentId, string ExternalReference);
