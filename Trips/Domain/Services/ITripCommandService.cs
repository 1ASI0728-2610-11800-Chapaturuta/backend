using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Commands;

namespace Frock_backend.Trips.Domain.Services;

public interface ITripCommandService
{
    Task<Trip?> Handle(CreateTripCommand command);
}
