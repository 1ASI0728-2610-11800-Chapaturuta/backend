using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.ValueObjects;

namespace Frock_backend.Tests.Trips.Domain;

public class ReservationAggregateTests
{
    private static Reservation NewReservation(int seats = 1, string documentNumber = "12345678")
    {
        return new Reservation(
            fkIdUser: 10,
            fkIdTrip: 20,
            documentType: DocumentType.Dni,
            documentNumber: documentNumber,
            seats: seats);
    }

    [Fact]
    public void Reservation_Ctor_Throws_When_Seats_Zero()
    {
        // ARRANGE + ACT + ASSERT
        var ex = Assert.Throws<ArgumentException>(() =>
            new Reservation(1, 1, DocumentType.Dni, "12345678", seats: 0));
        Assert.Equal("seats", ex.ParamName);
    }

    [Fact]
    public void Reservation_Ctor_Throws_When_DocumentNumber_Empty()
    {
        // ARRANGE + ACT + ASSERT
        var ex = Assert.Throws<ArgumentException>(() =>
            new Reservation(1, 1, DocumentType.Dni, documentNumber: "   ", seats: 2));
        Assert.Equal("documentNumber", ex.ParamName);
    }

    [Fact]
    public void AttachPayment_Sets_FkIdPayment_Without_Changing_Status()
    {
        // ARRANGE
        var reservation = NewReservation();
        var originalStatus = reservation.Status;

        // ACT
        reservation.AttachPayment(paymentId: 999);

        // ASSERT
        Assert.Equal(999, reservation.FkIdPayment);
        Assert.Equal(originalStatus, reservation.Status);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
    }

    [Fact]
    public void Confirm_Sets_Status_Confirmed_And_ConfirmedAt()
    {
        // ARRANGE
        var reservation = NewReservation();
        var before = DateTime.UtcNow.AddSeconds(-1);

        // ACT
        reservation.Confirm(paymentId: 555);

        // ASSERT
        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(555, reservation.FkIdPayment);
        Assert.NotNull(reservation.ConfirmedAt);
        Assert.True(reservation.ConfirmedAt!.Value >= before);
    }

    [Fact]
    public void Cancel_Sets_Status_Cancelled()
    {
        // ARRANGE
        var reservation = NewReservation();

        // ACT
        reservation.Cancel();

        // ASSERT
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }
}
