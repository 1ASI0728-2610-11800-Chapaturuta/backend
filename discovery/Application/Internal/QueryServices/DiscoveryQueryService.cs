using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Model.ValueObjects;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.ValueObjects;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.Trips.Domain.Repositories;

namespace Frock_backend.Discovery.Application.Internal.QueryServices;

public class DiscoveryQueryService(
    IRouteRepository routeRepository,
    IStopRepository stopRepository,
    ITripRepository tripRepository,
    IOsrmRoutingService osrmRoutingService) : IDiscoveryQueryService
{
    public async Task<IEnumerable<SearchRouteResult>> Handle(SearchRoutesQuery query)
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

        var results = new List<SearchRouteResult>();
        foreach (var route in activeRoutes)
        {
            double? distM = null;
            double? durS = null;

            if (!string.IsNullOrEmpty(query.Origin) && !string.IsNullOrEmpty(query.Destination))
            {
                var originStop = route.Stops.FirstOrDefault(s =>
                    s.Stop.Name.Contains(query.Origin!, StringComparison.OrdinalIgnoreCase) ||
                    s.Stop.Address.Contains(query.Origin!, StringComparison.OrdinalIgnoreCase));
                var destStop = route.Stops.LastOrDefault(s =>
                    s.Stop.Name.Contains(query.Destination!, StringComparison.OrdinalIgnoreCase) ||
                    s.Stop.Address.Contains(query.Destination!, StringComparison.OrdinalIgnoreCase));

                if (originStop?.Stop?.Latitude.HasValue == true && originStop.Stop.Longitude.HasValue &&
                    destStop?.Stop?.Latitude.HasValue == true && destStop.Stop.Longitude.HasValue)
                {
                    try
                    {
                        var osrmResult = await osrmRoutingService.RouteAsync(new[]
                        {
                            new Coordinate(originStop.Stop.Latitude.Value, originStop.Stop.Longitude.Value),
                            new Coordinate(destStop.Stop.Latitude.Value, destStop.Stop.Longitude.Value)
                        });
                        distM = osrmResult.DistanceMeters;
                        durS = osrmResult.DurationSeconds;
                    }
                    catch { /* OSRM unavailable — return route without estimates */ }
                }
            }

            results.Add(new SearchRouteResult(route, distM, durS));
        }

        return results;
    }

    public async Task<IEnumerable<Stop>> Handle(GetNearbyStopsQuery query, bool useRoadDistance = false)
    {
        var allStops = await stopRepository.ListAsync();
        var nearby = allStops.Where(s =>
            s.Latitude.HasValue && s.Longitude.HasValue &&
            CalculateDistanceKm(query.Latitude, query.Longitude, s.Latitude.Value, s.Longitude.Value) <= query.RadiusKm
        ).ToList();

        if (!useRoadDistance || nearby.Count == 0)
        {
            return nearby.OrderBy(s =>
                CalculateDistanceKm(query.Latitude, query.Longitude, s.Latitude!.Value, s.Longitude!.Value));
        }

        var source = new Coordinate(query.Latitude, query.Longitude);
        var destinations = nearby.Select(s => new Coordinate(s.Latitude!.Value, s.Longitude!.Value)).ToList();

        try
        {
            var durations = (await osrmRoutingService.TableAsync(source, destinations)).ToList();
            return nearby
                .Select((s, i) => (Stop: s, RoadDuration: i < durations.Count ? durations[i] : double.MaxValue))
                .OrderBy(x => x.RoadDuration)
                .Select(x => x.Stop);
        }
        catch
        {
            return nearby.OrderBy(s =>
                CalculateDistanceKm(query.Latitude, query.Longitude, s.Latitude!.Value, s.Longitude!.Value));
        }
    }

    public async Task<IEnumerable<RouteAggregate>> Handle(GetPopularRoutesQuery query)
    {
        var allTrips = await tripRepository.ListAsync();
        var topRouteIds = allTrips.GroupBy(t => t.FkIdRoute)
            .OrderByDescending(g => g.Count())
            .Take(query.Limit)
            .Select(g => g.Key)
            .ToList();

        var allRoutes = await routeRepository.ListRoutes();
        return allRoutes.Where(r => topRouteIds.Contains(r.Id) && r.IsActive);
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
            .OrderBy(x => x.Hour).ToList();

        var demandByDay = trips.GroupBy(t => t.StartTime.DayOfWeek)
            .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
            .OrderBy(x => x.Day).ToList();

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
