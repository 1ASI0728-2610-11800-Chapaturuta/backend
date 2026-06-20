using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.IAM.Infrastructure.Persistence.EFC.Repositories;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Interfaces.ACL;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Infrastructure.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Infrastructure.Repositories;
using Frock_backend.Trips.Application.Internal.CommandServices;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace Frock_backend.Tests.Trips.Application;

/// <summary>
/// Integration tests for the create-trip -> create-reservation flow.
/// Exercises the real command services + EF repositories + AppDbContext (EF InMemory),
/// mocking only the Payments BC facade. Guards against regressions in the flow that
/// previously surfaced as opaque HTTP 400s.
/// </summary>
public class TripReservationFlowTests
{
    private static AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    ///     Seeds the FK targets that TripCommandService validates (user, route, two stops) so the
    ///     create-trip flow reaches persistence. EF InMemory assigns the first int key as 1, etc.
    /// </summary>
    private static void SeedReferences(AppDbContext context)
    {
        context.Set<User>().Add(new User("t@t.com", "tester", "hash", Frock_backend.IAM.Domain.Model.ValueObjects.Role.Traveller)); // -> id 1
        context.Set<RouteAggregate>().Add(new RouteAggregate(10.0, 30, 30) { Id = 1 });
        context.Set<Stop>().Add(new Stop(1, "Origen", "Av. Origen", 1, 1) { Reference = "ref" });
        context.Set<Stop>().Add(new Stop(2, "Destino", "Av. Destino", 1, 1) { Reference = "ref" });
        context.SaveChanges();
    }

    private static (TripCommandService trips, ReservationCommandService reservations, Mock<IPaymentsContextFacade> payments)
        BuildServices(AppDbContext context)
    {
        SeedReferences(context);

        var unitOfWork = new UnitOfWork(context);
        var tripRepo = new TripRepository(context);
        var reservationRepo = new ReservationRepository(context);
        var userRepo = new UserRepository(context);
        var routeRepo = new RouteRepository(context);
        var stopRepo = new StopRepository(context);
        var payments = new Mock<IPaymentsContextFacade>();
        payments
            .Setup(p => p.RegisterPendingPaymentAsync(
                It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(),
                It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(777);

        var driverFacade = new Mock<IDriverContextFacade>();
        var tripService = new TripCommandService(tripRepo, userRepo, routeRepo, stopRepo, driverFacade.Object, unitOfWork);
        var reservationService = new ReservationCommandService(
            reservationRepo, tripRepo, userRepo, payments.Object, unitOfWork,
            Options.Create(new ReservationHoldOptions { PaymentHoldMinutes = 15 }));
        return (tripService, reservationService, payments);
    }

    [Fact]
    public async Task CreateTrip_Then_CreateReservation_HappyPath_Persists_And_Decrements_Seats()
    {
        // ARRANGE
        using var context = NewContext();
        var (trips, reservations, payments) = BuildServices(context);

        // ACT — create trip
        var trip = await trips.Handle(new CreateTripCommand(
            FkIdUser: 1, FkIdDriver: null, FkIdRoute: 1,
            FkIdOriginStop: 1, FkIdDestinationStop: 2, Price: 10.5m, AvailableSeats: 5));

        // ASSERT — trip persisted
        Assert.NotNull(trip);
        Assert.True(trip!.Id > 0);
        Assert.Equal(5, trip.AvailableSeats);
        Assert.NotNull(await context.Set<Trip>().FindAsync(trip.Id));

        // ACT — create reservation against that trip
        var reservation = await reservations.Handle(new CreateReservationCommand(
            FkIdUser: 1, FkIdTrip: trip.Id, DocumentType: DocumentType.Dni,
            DocumentNumber: "12345678", Seats: 2, PaymentMethod: PaymentMethod.Cash));

        // ASSERT — reservation persisted, seats decremented, payment attached
        Assert.NotNull(reservation);
        Assert.True(reservation!.Id > 0);
        Assert.Equal(2, reservation.Seats);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(777, reservation.FkIdPayment);
        Assert.Equal(3, trip.AvailableSeats); // 5 - 2

        var persisted = await context.Set<Reservation>().FindAsync(reservation.Id);
        Assert.NotNull(persisted);
        Assert.Equal(trip.Id, persisted!.FkIdTrip);

        payments.Verify(p => p.RegisterPendingPaymentAsync(
            1, 21m, PaymentMethod.Cash, "Reservation", reservation.Id), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_On_Missing_Trip_Throws_InvalidOperation()
    {
        // ARRANGE
        using var context = NewContext();
        var (_, reservations, _) = BuildServices(context);

        // ACT + ASSERT — no trip with id 999 -> propagates (maps to 409 via GlobalExceptionHandler, not a masked 400)
        var ex = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            reservations.Handle(new CreateReservationCommand(
                FkIdUser: 1, FkIdTrip: 999, DocumentType: DocumentType.Dni,
                DocumentNumber: "12345678", Seats: 1, PaymentMethod: PaymentMethod.Cash)));
        Assert.Contains("Trip with id 999 not found", ex.Message);
    }

    [Fact]
    public async Task CreateReservation_With_Insufficient_Seats_Throws_And_Does_Not_Persist()
    {
        // ARRANGE
        using var context = NewContext();
        var (trips, reservations, payments) = BuildServices(context);
        var trip = await trips.Handle(new CreateTripCommand(
            FkIdUser: 1, FkIdDriver: null, FkIdRoute: 1,
            FkIdOriginStop: 1, FkIdDestinationStop: 2, Price: 10m, AvailableSeats: 1));

        // ACT + ASSERT — asking for 2 seats when only 1 is available
        await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
            reservations.Handle(new CreateReservationCommand(
                FkIdUser: 1, FkIdTrip: trip!.Id, DocumentType: DocumentType.Dni,
                DocumentNumber: "12345678", Seats: 2, PaymentMethod: PaymentMethod.Cash)));

        Assert.Equal(1, trip!.AvailableSeats);              // unchanged
        Assert.Empty(await context.Set<Reservation>().ToListAsync());
        payments.Verify(p => p.RegisterPendingPaymentAsync(
            It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<PaymentMethod>(),
            It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }
}
