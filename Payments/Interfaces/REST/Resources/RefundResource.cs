namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record RefundResource(
    int Id,
    int FkIdPayment,
    decimal Amount,
    string Currency,
    string Reason,
    string Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt
);
