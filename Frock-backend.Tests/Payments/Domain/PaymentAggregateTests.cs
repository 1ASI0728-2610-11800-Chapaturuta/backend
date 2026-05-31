using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Payments.Domain;

public class PaymentAggregateTests
{
    private static Payment NewPayment(decimal amount = 100m)
    {
        return new Payment(
            fkIdUser: 1,
            amount: new Money(amount, "PEN"),
            method: PaymentMethod.Yape,
            referenceType: "Reservation",
            referenceId: 10);
    }

    [Fact]
    public void Confirm_Sets_Status_Completed_And_ExternalReference_And_ConfirmedAt()
    {
        // ARRANGE
        var payment = NewPayment();
        var before = DateTime.UtcNow.AddSeconds(-1);

        // ACT
        payment.Confirm("EXT-123");

        // ASSERT
        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("EXT-123", payment.ExternalReference);
        Assert.NotNull(payment.ConfirmedAt);
        Assert.True(payment.ConfirmedAt!.Value >= before);
    }

    [Fact]
    public void Fail_Sets_Status_Failed()
    {
        // ARRANGE
        var payment = NewPayment();

        // ACT
        payment.Fail();

        // ASSERT
        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public void MarkRefunded_Full_Sets_Status_Refunded()
    {
        // ARRANGE
        var payment = NewPayment();
        payment.Confirm("EXT-FULL");

        // ACT
        payment.MarkRefunded(partial: false);

        // ASSERT
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
    }

    [Fact]
    public void MarkRefunded_Partial_Sets_Status_PartiallyRefunded()
    {
        // ARRANGE
        var payment = NewPayment();
        payment.Confirm("EXT-PART");

        // ACT
        payment.MarkRefunded(partial: true);

        // ASSERT
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
    }
}
