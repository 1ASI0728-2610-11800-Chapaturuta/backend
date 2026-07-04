namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record PaymentResource(
    int Id,
    int FkIdUser,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? ExternalReference,
    string ReferenceType,
    int ReferenceId,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    // Display-only name of the user who made the payment. Never persisted — resolved on read.
    string? PayerName = null
);
