namespace Frock_backend.Payments.Domain.Model.ValueObjects;

public enum PaymentStatus
{
    Pending,
    Completed,
    Failed,
    Refunded,
    PartiallyRefunded
}
