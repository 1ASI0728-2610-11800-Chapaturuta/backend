using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;

namespace Frock_backend.Trips.Domain.Services;

public interface ITripQueryService
{
    Task<Trip?> Handle(GetTripByIdQuery query);
    Task<IEnumerable<Trip>> Handle(GetTripsByUserIdQuery query);
    Task<IEnumerable<Trip>> Handle(GetTripsByDriverIdQuery query);
}
