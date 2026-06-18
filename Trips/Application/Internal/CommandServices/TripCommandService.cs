using Frock_backend.IAM.Domain.Repositories;
using Frock_backend.routes.Domain.Repository;
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
    IUnitOfWork unitOfWork) : ITripCommandService
{
    public async Task<Trip?> Handle(CreateTripCommand command)
    {
        // Validate referenced entities exist BEFORE persisting. Otherwise a missing FK
        // surfaces as a MySQL FK-constraint DbUpdateException → HTTP 500. We throw
        // KeyNotFoundException, which GlobalExceptionHandler maps to a controlled 404.
        if (await userRepository.FindByIdAsync(command.FkIdUser) is null)
            throw new KeyNotFoundException($"User with id {command.FkIdUser} not found");

        if (await routeRepository.FindByIdAsync(command.FkIdRoute) is null)
            throw new KeyNotFoundException($"Route with id {command.FkIdRoute} not found");

        if (await stopRepository.FindByIdAsync(command.FkIdOriginStop) is null)
            throw new KeyNotFoundException($"Origin stop with id {command.FkIdOriginStop} not found");

        if (await stopRepository.FindByIdAsync(command.FkIdDestinationStop) is null)
            throw new KeyNotFoundException($"Destination stop with id {command.FkIdDestinationStop} not found");

        var trip = new Trip(command.FkIdUser, command.FkIdDriver, command.FkIdRoute, command.FkIdOriginStop, command.FkIdDestinationStop, command.Price, command.AvailableSeats);

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
