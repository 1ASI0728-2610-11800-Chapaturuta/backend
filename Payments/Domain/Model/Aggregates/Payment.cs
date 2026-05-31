using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Payments.Domain.Model.Aggregates;

public class Payment
{
    public int Id { get; }
    public int FkIdUser { get; set; }
    public Money Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? ExternalReference { get; set; }
    public string ReferenceType { get; set; }
    public int ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    protected Payment()
    {
        Amount = new Money(0);
        ReferenceType = string.Empty;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Payment(int fkIdUser, Money amount, PaymentMethod method, string referenceType, int referenceId)
    {
        FkIdUser = fkIdUser;
        Amount = amount;
        Method = method;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm(string externalRef)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm a payment with status {Status}");
        ExternalReference = externalRef;
        Status = PaymentStatus.Completed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail a payment with status {Status}");
        Status = PaymentStatus.Failed;
    }

    public void MarkRefunded(bool partial)
    {
        if (Status != PaymentStatus.Completed && Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException($"Cannot refund a payment with status {Status}");
        Status = partial ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
    }
}
