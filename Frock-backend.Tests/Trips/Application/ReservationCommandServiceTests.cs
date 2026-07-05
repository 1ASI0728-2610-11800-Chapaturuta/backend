using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.Trips.Application.Internal.CommandServices;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;

namespace Frock_backend.Tests.Trips.Application;

public class ReservationCommandServiceTests
{
    private readonly Mock<IReservationRepository> _reservationRepo = new(MockBehavior.Strict);
    private readonly Mock<ITripRepository> _tripRepo = new(MockBehavior.Strict);
    private readonly Mock<IRouteRepository> _routeRepo = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _userRepo = new(MockBehavior.Strict);
    private readonly Mock<IPaymentsContextFacade> _paymentsFacade = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unitOfWork = new(MockBehavior.Strict);
    private const int HoldMinutes = 15;

    private ReservationCommandService BuildService() => new(
        _reservationRepo.Object,
        _tripRepo.Object,
        _routeRepo.Object,
        _userRepo.Object,
        _paymentsFacade.Object,
        _unitOfWork.Object,
        Options.Create(new ReservationHoldOptions { PaymentHoldMinutes = HoldMinutes }));

    // NewTrip() always uses fkIdRoute: 3 with no configured Schedules -> RouteScheduleRules.IsOpenAt
    // treats it as always open, so this route repo setup is a no-op safety pass for that check.
    private void SetupOpenRoute() =>
        _routeRepo.Setup(r => r.FindByRouteId(3)).ReturnsAsync(new RouteAggregate(10.0, 30, 30) { Id = 3 });

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
        _userRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new User());
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
        _userRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(new User());
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        SetupOpenRoute();
        _reservationRepo.Setup(r => r.FindByTripIdAsync(trip.Id)).ReturnsAsync(new List<Reservation>());
        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 1, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 2, PaymentMethod: PaymentMethod.Yape);

        // ACT + ASSERT
        // Trip.ReserveSeats throws InvalidOperationException when seats are insufficient; it propagates.
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Handle(cmd));
        Assert.Equal(1, trip.AvailableSeats); // unchanged
    }

    [Fact]
    public async Task CreateReservation_Happy_Path()
    {
        // ARRANGE
        var trip = NewTrip(availableSeats: 5, price: 10.0m);
        _userRepo.Setup(r => r.FindByIdAsync(42)).ReturnsAsync(new User());
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        SetupOpenRoute();
        _reservationRepo.Setup(r => r.FindByTripIdAsync(trip.Id)).ReturnsAsync(new List<Reservation>());

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

    private static Reservation PendingHold(int userId, int tripId, int seats, int paymentId, DateTime reservedAt)
    {
        var r = new Reservation(userId, tripId, DocumentType.Dni, "12345678", seats) { ReservedAt = reservedAt };
        r.AttachPayment(paymentId);
        return r;
    }

    [Fact]
    public async Task CreateReservation_Releases_Expired_Hold_Before_Reserving()
    {
        // ARRANGE: a different user's pending hold whose 15-min payment window elapsed (reserved 20 min ago).
        var expired = PendingHold(userId: 7, tripId: 1, seats: 2, paymentId: 500,
            reservedAt: DateTime.UtcNow.AddMinutes(-20));
        var trip = NewTrip(availableSeats: 0, price: 10.0m); // fully held by the expired reservation

        _userRepo.Setup(r => r.FindByIdAsync(42)).ReturnsAsync(new User());
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        SetupOpenRoute();
        _reservationRepo.Setup(r => r.FindByTripIdAsync(trip.Id))
            .ReturnsAsync(new List<Reservation> { expired });
        _reservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        _reservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()));
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _paymentsFacade.Setup(p => p.FailPaymentAsync(500)).Returns(Task.CompletedTask);
        _paymentsFacade
            .Setup(p => p.RegisterPendingPaymentAsync(42, 10m, PaymentMethod.Yape, "Reservation", It.IsAny<int>()))
            .ReturnsAsync(123);

        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 42, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 1, PaymentMethod: PaymentMethod.Yape);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Expired, expired.Status);   // hold released
        Assert.Equal(1, trip.AvailableSeats);                      // 0 -> +2 (released) -> -1 (new)
        _paymentsFacade.Verify(p => p.FailPaymentAsync(500), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_Supersedes_Same_User_Pending_Hold()
    {
        // ARRANGE: same user already has a still-fresh (not expired) pending hold on this trip.
        var previous = PendingHold(userId: 42, tripId: 1, seats: 2, paymentId: 600,
            reservedAt: DateTime.UtcNow); // within the window
        var trip = NewTrip(availableSeats: 0, price: 10.0m);

        _userRepo.Setup(r => r.FindByIdAsync(42)).ReturnsAsync(new User());
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        SetupOpenRoute();
        _reservationRepo.Setup(r => r.FindByTripIdAsync(trip.Id))
            .ReturnsAsync(new List<Reservation> { previous });
        _reservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        _reservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()));
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _paymentsFacade.Setup(p => p.FailPaymentAsync(600)).Returns(Task.CompletedTask);
        _paymentsFacade
            .Setup(p => p.RegisterPendingPaymentAsync(42, 10m, PaymentMethod.Yape, "Reservation", It.IsAny<int>()))
            .ReturnsAsync(123);

        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 42, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 1, PaymentMethod: PaymentMethod.Yape);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT: the prior unpaid hold is released so the retry does not stack a second hold.
        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Expired, previous.Status);
        Assert.Equal(1, trip.AvailableSeats);                      // 0 -> +2 -> -1
        _paymentsFacade.Verify(p => p.FailPaymentAsync(600), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_Keeps_Other_Users_Active_Hold()
    {
        // ARRANGE: another user holds a fresh, unexpired reservation — it must NOT be released.
        var otherActive = PendingHold(userId: 99, tripId: 1, seats: 1, paymentId: 700,
            reservedAt: DateTime.UtcNow);
        var trip = NewTrip(availableSeats: 1, price: 10.0m); // 1 free seat left besides the other hold

        _userRepo.Setup(r => r.FindByIdAsync(42)).ReturnsAsync(new User());
        _tripRepo.Setup(r => r.FindByIdAsync(1)).ReturnsAsync(trip);
        SetupOpenRoute();
        _reservationRepo.Setup(r => r.FindByTripIdAsync(trip.Id))
            .ReturnsAsync(new List<Reservation> { otherActive });
        _reservationRepo.Setup(r => r.AddAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);
        _reservationRepo.Setup(r => r.Update(It.IsAny<Reservation>()));
        _unitOfWork.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _paymentsFacade
            .Setup(p => p.RegisterPendingPaymentAsync(42, 10m, PaymentMethod.Yape, "Reservation", It.IsAny<int>()))
            .ReturnsAsync(123);

        var service = BuildService();
        var cmd = new CreateReservationCommand(
            FkIdUser: 42, FkIdTrip: 1, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 1, PaymentMethod: PaymentMethod.Yape);

        // ACT
        var result = await service.Handle(cmd);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Pending, otherActive.Status);   // untouched
        Assert.Equal(0, trip.AvailableSeats);                          // 1 -> -1 (new), other hold intact
        _paymentsFacade.Verify(p => p.FailPaymentAsync(It.IsAny<int>()), Times.Never);
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
