using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.Trips.Application.Internal.CommandServices;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Repositories;
using Moq;

namespace Frock_backend.Tests.Trips.Application;

public class ReservationCommandServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new(MockBehavior.Strict);
    private readonly Mock<ITripRepository> _tripRepo = new(MockBehavior.Strict);
    private readonly Mock<IPaymentsContextFacade> _paymentsFacade = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);

    private ReservationCommandService BuildService() => new(
        _reservationRepo.Object,
        _tripRepo.Object,
        _paymentsFacade.Object,
        _unitOfWork.Object);

    private static Trip NewTrip(int availableSeats, decimal price = 10.0m) =>
        new(
            fkIdUser: 1,
            fkIdDriver: 2,
            fkIdRoute: 3,
            fkIdOriginStop: 4,
            fkIdDestinationStop: 5,
            price: price,
            availableSeats: availableSeats);

    [Fact]
    public async Task CreateReservation_Throws_When_Trip_Not_Found()
    {
        // ARRANGE
        _tripRepo.Setup(r => r.FindByIdAsync(99)).ReturnsAsync((Trip?)null);
        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 1, FkIdTrip: 99, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 1, PaymentMethod: PaymentMethod.Yape);

        // ACT + ASSERT
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Handle(cmd));
        Assert.Contains("Trip with id 99 not found", ex.Message);
        _tripRepo.Verify(r => r.FindByIdAsync(99), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_Throws_When_Not_Enough_Seats()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 1);
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 1, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 2, PaymentMethod: PaymentMethod.Yape);

        // ACT + ASSERT
        // Trip.ReserveSeats throws InvalidOperationException; happens BEFORE the try/catch, so it propagates.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Handle(cmd));
        Assert.Equal(1, trip.AvailableSeats); // unchanged
    }

    [Fact]
    public async Task CreateReservation_Happy_Path()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 5, price: 10.0m);
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);

        Reservation? captured = null;
        _reservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>()))
            .Callback<Reservation>(r => captured = r)
            .Returns(Task.CompletedTask);
        _reservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()));

        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        _paymentsFacade
            .Setup(p => p.RegisterPendingPaymentAsync(
                42, 20m, PaymentMethod.Yape, "Reservation", It.IsAny<int>()))
            .ReturnsAsync(123);

        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 42, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 2, PaymentMethod: PaymentMethod.Yape);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT
        Assert.NotNull(result);
        Assert.Same(captured, result);
        Assert.Equal(3, trip.AvailableSeats);                       // 5 - 2
        Assert.Equal(123, result!.FkIdPayment);                     // payment attached
        Assert.Equal(ReservationStatus.Pending, result.Status);     // still pending
        Assert.Equal(2, result.Seats);
        Assert.Equal(42, result.FkIdUser);
        Assert.Equal(1, result.FkIdTrip);

        _paymentsFacade.Verify(p => p.RegisterPendingPaymentAsync(
            42, 20m, PaymentMethod.Yape, "Reservation", It.IsAny<int>()), Times.Once);
        _reservationRepo.Verify(r => r.AddAsync(It.IsAny<Reservation>()), Times.Once);
        _reservationRepo.Verify(r => r.Update(It.IsAny<Reservation>()), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task CancelReservation_Releases_Seats_And_Requests_Refund_When_Was_Confirmed()
    {
        // ARRANGE
        var reservation = new Reservation(
            fkIdUser: 7, fkIdTrip: 50, documentType: DocumentType.Dni,
            documentNumber: "12345678", seats: 2);
        reservation.Confirm(paymentId: 123); // status = Confirmed, FkIdPayment = 123

        var trip = NewTrip(availableSeats: 0, price: 10.0m);

        _reservationRepo.Setup(r => r.FindByIdAsync(500)).ReturnsAsync(reservation);
        _tripRepo.Setup(r => r.FindByIdAsync(50)).ReturnsAsync(trip);
        _reservationRepo.Setup(r => r.Update(reservation));
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        _paymentsFacade
            .Setup(p => p.RegisterRefundAsync(123, It.IsAny<decimal>(), It.IsAny<string>()))
            .ReturnsAsync(999);

        var service = BuildService();
        var cmd = new CancelReservationCommand(ReservationId: 500);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(2, trip.AvailableSeats);                          // released
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        _paymentsFacade.Verify(
            p => p.RegisterRefundAsync(123, It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Once);
        _reservationRepo.Verify(r => r.Update(reservation), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelReservation_Skips_Refund_When_Was_Pending()
    {
        // ARRANGE: Pending reservation (no payment attached yet)
        var reservation = new Reservation(
            fkIdUser: 7, fkIdTrip: 50, documentType: DocumentType.Dni,
            documentNumber: "12345678", seats: 2);
        // Pending by default; FkIdPayment is null.

        var trip = NewTrip(availableSeats: 0, price: 10.0m);

        _reservationRepo.Setup(r => r.FindByIdAsync(500)).ReturnsAsync(reservation);
        _tripRepo.Setup(r => r.FindByIdAsync(50)).ReturnsAsync(trip);
        _reservationRepo.Setup(r => r.Update(reservation));
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        var service = BuildService();
        var cmd = new CancelReservationCommand(ReservationId: 500);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(2, trip.AvailableSeats);
        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);

        _paymentsFacade.Verify(
            p => p.RegisterRefundAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>()),
            Times.Never);
    }
}
