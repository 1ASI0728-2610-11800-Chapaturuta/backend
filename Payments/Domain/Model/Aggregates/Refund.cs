using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Payments.Domain.Model.Aggregates;

public class Refund
{
    public int Id { get; }
    public int FkIdPayment { get; set; }
    public Money Amount { get; set; }
    public string Reason { get; set; }
    public RefundStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    protected Refund()
    {
        Amount = new Money(0);
        Reason = string.Empty;
        Status = RefundStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Refund(int fkIdPayment, Money amount, string reason)
    {
        FkIdPayment = fkIdPayment;
        Amount = amount;
        Reason = reason;
        Status = RefundStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != RefundStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm a refund with status {Status}");
        Status = RefundStatus.Completed;
        ConfirmedAt = DateTime.UtcNow;
    }
}
