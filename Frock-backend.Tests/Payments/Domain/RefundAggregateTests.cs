using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Payments.Domain;

public class RefundAggregateTests
{
    [Fact]
    public void Confirm_Sets_Status_Completed_And_ConfirmedAt()
    {
        // ARRANGE
        var refund = new Refund(
            fkIdPayment: 1,
            amount: new Money(50m, "PEN"),
            reason: "customer request");
        var before = DateTime.UtcNow.AddSeconds(-1);

        // ACT
        refund.Confirm();

        // ASSERT
        Assert.Equal(RefundStatus.Completed, refund.Status);
        Assert.NotNull(refund.ConfirmedAt);
        Assert.True(refund.ConfirmedAt!.Value >= before);
    }
}
