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
        var trip = new Trip(command.FkIdUser, command.FkIdDriver, command.FkIdRoute, command.FkIdOriginStop, command.FkIdDestinationStop, command.Price);

        try
        {
            await tripRepository.AddAsync(trip);
            await unitOfWork.CompleteAsync();
            return trip;
        }
        catch (Exception e)
        {
            throw new Exception($"An error occurred while creating trip: {e.Message}");
        }
    }
}
