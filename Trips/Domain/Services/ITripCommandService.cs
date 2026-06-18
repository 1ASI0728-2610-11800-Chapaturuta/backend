using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;

namespace Frock_backend.Trips.Domain.Services;

public interface ITripCommandService
{
    Task<Trip?> Handle(CreateTripCommand command);
    Task<Trip?> Handle(AssignDriverToTripCommand command);
    Task<Trip?> Handle(StartTripCommand command);
    Task<Trip?> Handle(CompleteTripCommand command);
    Task<Trip?> Handle(CancelTripCommand command);
}
