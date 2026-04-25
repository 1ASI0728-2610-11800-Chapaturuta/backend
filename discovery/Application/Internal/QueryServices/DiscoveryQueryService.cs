using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.Trips.Domain.Repositories;

namespace Frock_backend.Discovery.Application.Internal.QueryServices;

public class DiscoveryQueryService(
    IRouteRepository routeRepository,
    IStopRepository stopRepository,
    ITripRepository tripRepository) : IDiscoveryQueryService
{
    public async Task<IEnumerable<RouteAggregate>> Handle(SearchRoutesQuery query)
    {
        var allRoutes = await routeRepository.ListRoutes();
        var activeRoutes = allRoutes.Where(r => r.IsActive).ToList();

        if (!string.IsNullOrEmpty(query.Origin) || !string.IsNullOrEmpty(query.Destination))
        {
            activeRoutes = activeRoutes.Where(r =>
                r.Stops.Any(s =>
                    (string.IsNullOrEmpty(query.Origin) || s.Stop.Name.Contains(query.Origin, StringComparison.OrdinalIgnoreCase) || s.Stop.Address.Contains(query.Origin, StringComparison.OrdinalIgnoreCase)) ||
                    (string.IsNullOrEmpty(query.Destination) || s.Stop.Name.Contains(query.Destination, StringComparison.OrdinalIgnoreCase) || s.Stop.Address.Contains(query.Destination, StringComparison.OrdinalIgnoreCase))
                )).ToList();
        }

        return activeRoutes;
    }

    public async Task<IEnumerable<Stop>> Handle(GetNearbyStopsQuery query)
    {
        var allStops = await stopRepository.ListAsync();
        return allStops.Where(s =>
            s.Latitude.HasValue && s.Longitude.HasValue &&
            CalculateDistanceKm(query.Latitude, query.Longitude, s.Latitude.Value, s.Longitude.Value) <= query.RadiusKm
        ).OrderBy(s =>
            CalculateDistanceKm(query.Latitude, query.Longitude, s.Latitude!.Value, s.Longitude!.Value)
        );
    }

    public async Task<IEnumerable<RouteAggregate>> Handle(GetPopularRoutesQuery query)
    {
        var allTrips = await tripRepository.ListAsync();
        var tripsByRoute = allTrips.GroupBy(t => t.FkIdRoute)
            .OrderByDescending(g => g.Count())
            .Take(query.Limit)
            .Select(g => g.Key)
            .ToList();

        var allRoutes = await routeRepository.ListRoutes();
        return allRoutes.Where(r => tripsByRoute.Contains(r.Id) && r.IsActive);
    }

    public async Task<object> Handle(GetDemandAnalyticsQuery query)
    {
        var allTrips = await tripRepository.ListAsync();
        var trips = allTrips.AsEnumerable();

        if (query.DistrictId.HasValue)
        {
            var stopsInDistrict = await stopRepository.FindByFkIdDistrictAsync(query.DistrictId.Value);
            var stopIds = stopsInDistrict.Select(s => s.Id).ToHashSet();
            trips = trips.Where(t => stopIds.Contains(t.FkIdOriginStop) || stopIds.Contains(t.FkIdDestinationStop));
        }

        var demandByHour = trips.GroupBy(t => t.StartTime.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToList();

        var demandByDay = trips.GroupBy(t => t.StartTime.DayOfWeek)
            .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
            .OrderBy(x => x.Day)
            .ToList();

        return new
        {
            districtId = query.DistrictId,
            period = query.Period ?? "all",
            totalTrips = trips.Count(),
            demandByHour,
            demandByDay
        };
    }

    private static double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
