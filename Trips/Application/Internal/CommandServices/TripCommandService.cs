using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.Trips.Application.Internal.CommandServices;

public class TripCommandService(ITripRepository tripRepository, IUnitOfWork unitOfWork) : ITripCommandService
{
    public async Task<Trip?> Handle(CreateTripCommand command)
    {
        var trip = new Trip(command.FkIdUser, command.FkIdDriver, command.FkIdRoute, command.FkIdOriginStop, command.FkIdDestinationStop, command.Price, command.AvailableSeats);

        // Don't re-wrap persistence failures in a bare Exception: that discarded the
        // DbUpdateException (and its InnerException with the real SQL error) and made the
        // controller return a generic 400. Let it propagate so it's logged and mapped properly.
        await tripRepository.AddAsync(trip);
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
