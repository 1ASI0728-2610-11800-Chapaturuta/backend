using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Trips.Application.Internal.CommandServices;

public class TripCommandService(
    ITripRepository tripRepository,
    IUserRepository userRepository,
    IRouteRepository routeRepository,
    IStopRepository stopRepository,
    IDriverContextFacade driverContextFacade,
    IUnitOfWork unitOfWork) : ITripCommandService
{
    // Verifies the route belongs to the driver: route ownership = any of the route's stops is owned
    // by the driver (Stop.FkIdDriver). Resolves the driver from the authenticated user first.
    private async Task<int> EnsureDriverOwnsTripRouteAsync(int requestingUserId, Trip trip)
    {
        var driverId = await driverContextFacade.FetchDriverIdByUserIdAsync(requestingUserId);
        if (driverId is null)
            throw new InvalidOperationException("No existe un conductor asociado a este usuario");

        var ownedRoutes = await routeRepository.FindByDriverId(driverId.Value);
        if (ownedRoutes.All(r => r.Id != trip.FkIdRoute))
            throw new InvalidOperationException("No puedes operar un viaje de una ruta que no te pertenece");

        return driverId.Value;
    }

    public async Task<Trip?> Handle(CreateTripCommand command)
    {
        // Validate referenced entities exist BEFORE persisting. Otherwise a missing FK
        // surfaces as a MySQL FK-constraint DbUpdateException → HTTP 500. We throw
        // KeyNotFoundException, which GlobalExceptionHandler maps to a controlled 404.
        if (await userRepository.FindByIdAsync(command.FkIdUser) is null)
            throw new KeyNotFoundException($"User with id {command.FkIdUser} not found");

        // Loaded via FindByRouteId (not FindByIdAsync) because it eager-loads Schedules, which
        // RouteScheduleRules.IsOpenAt needs below to validate the route's attention hours.
        var route = await routeRepository.FindByRouteId(command.FkIdRoute);
        if (route is null)
            throw new KeyNotFoundException($"Route with id {command.FkIdRoute} not found");

        if (await stopRepository.FindByIdAsync(command.FkIdOriginStop) is null)
            throw new KeyNotFoundException($"Origin stop with id {command.FkIdOriginStop} not found");

        if (await stopRepository.FindByIdAsync(command.FkIdDestinationStop) is null)
            throw new KeyNotFoundException($"Destination stop with id {command.FkIdDestinationStop} not found");

        // Seat capacity is NOT a free-form field for published trips: it must come from the driver's
        // registered vehicle (Driver.Vehicle.Capacity). When a driver is set (publish flow), the
        // vehicle capacity overrides whatever the client sent. Passenger-created requests
        // (FkIdDriver null) keep the supplied seat count.
        var availableSeats = command.AvailableSeats;
        if (command.FkIdDriver is > 0)
        {
            var capacity = await driverContextFacade.FetchVehicleCapacityByDriverIdAsync(command.FkIdDriver.Value);
            if (capacity is > 0) availableSeats = capacity.Value;
        }

        // The chosen start time (or "now" if none was given) must fall within the route's
        // configured attention hours (Schedules), evaluated in Lima local time.
        var effectiveStart = command.StartTimeUtc ?? DateTime.UtcNow;
        if (!RouteScheduleRules.IsOpenAt(route, effectiveStart, out var reason))
            throw new InvalidOperationException(reason);

        var trip = new Trip(command.FkIdUser, command.FkIdDriver, command.FkIdRoute, command.FkIdOriginStop, command.FkIdDestinationStop, command.Price, availableSeats, startTime: effectiveStart);

        // Don't re-wrap persistence failures in a bare Exception: that discarded the
        // DbUpdateException (and its InnerException with the real SQL error) and made the
        // controller return a generic 400. Let it propagate so it's logged and mapped properly.
        await tripRepository.AddAsync(trip);
        await unitOfWork.CompleteAsync();
        return trip;
    }

    public async Task<Trip?> Handle(AssignDriverToTripCommand command)
    {
        var trip = await tripRepository.FindByIdAsync(command.TripId);
        if (trip == null) return null;

        // A driver can only claim trips on routes they own.
        var ownedRoutes = await routeRepository.FindByDriverId(command.DriverId);
        if (ownedRoutes.All(r => r.Id != trip.FkIdRoute))
            throw new InvalidOperationException("No puedes reclamar un viaje de una ruta que no te pertenece");

        // Domain guards reject claiming a non-pending or already-assigned trip; let the
        // InvalidOperationException surface so the controller returns a controlled 400.
        trip.AssignDriver(command.DriverId);
        tripRepository.Update(trip);
        await unitOfWork.CompleteAsync();
        return trip;
    }

    public async Task<Trip?> Handle(StartTripCommand command)
    {
        var trip = await tripRepository.FindByIdAsync(command.TripId);
        if (trip == null) return null;

        // A driver can only start trips on routes they own.
        await EnsureDriverOwnsTripRouteAsync(command.RequestingUserId, trip);

        trip.Start();
        tripRepository.Update(trip);
        await unitOfWork.CompleteAsync();
        return trip;
    }

    public async Task<Trip?> Handle(CompleteTripCommand command)
    {
        var trip = await tripRepository.FindByIdAsync(command.TripId);
        if (trip == null) return null;

        trip.Complete();
        tripRepository.Update(trip);
        await unitOfWork.CompleteAsync();
        return trip;
    }

    public async Task<Trip?> Handle(CancelTripCommand command)
    {
        var trip = await tripRepository.FindByIdAsync(command.TripId);
        if (trip == null) return null;

        trip.Cancel();
        tripRepository.Update(trip);
        await unitOfWork.CompleteAsync();
        return trip;
    }
}
