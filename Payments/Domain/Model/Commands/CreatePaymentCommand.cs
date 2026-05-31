using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Payments.Domain.Model.Commands;

public record CreatePaymentCommand(
    int FkIdUser,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    string ReferenceType,
    int ReferenceId
);
