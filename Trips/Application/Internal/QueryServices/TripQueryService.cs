using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;

namespace Frock_backend.Trips.Application.Internal.QueryServices;

public class TripQueryService(ITripRepository tripRepository) : ITripQueryService
{
    public async Task<Trip?> Handle(GetTripByIdQuery query)
    {
        return await tripRepository.FindByIdAsync(query.Id);
    }

    public async Task<IEnumerable<Trip>> Handle(GetTripsByUserIdQuery query)
    {
        return await tripRepository.FindByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<Trip>> Handle(GetTripsByDriverIdQuery query)
    {
        return await tripRepository.FindByDriverIdAsync(query.DriverId);
    }
}
