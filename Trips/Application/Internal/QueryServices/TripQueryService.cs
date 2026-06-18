using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Model.ValueObjects;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.Driver.Interfaces.ACL;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Trips.Application.Internal.QueryServices;

public class TripQueryService(ITripRepository tripRepository, AppDbContext context, IDriverContextFacade driverContextFacade) : ITripQueryService
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

    public async Task<IEnumerable<TripHistoryView>> Handle(GetTripHistoryByUserIdQuery query)
    {
        var trips = await context.Set<Trip>()
            .Where(t => t.FkIdUser == query.UserId)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
        return await BuildHistoryAsync(trips);
    }

    public async Task<IEnumerable<TripHistoryView>> Handle(GetTripHistoryByDriverIdQuery query)
    {
        var trips = await context.Set<Trip>()
            .Where(t => t.FkIdDriver == query.DriverId)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
        return await BuildHistoryAsync(trips);
    }

    public async Task<IEnumerable<TripHistoryView>> Handle(GetAvailableTripsQuery query)
    {
        var trips = await context.Set<Trip>()
            .Where(t => t.FkIdDriver == null && t.Status == TripStatus.Pending)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
        return await BuildHistoryAsync(trips);
    }

    public async Task<IEnumerable<TripHistoryView>> Handle(GetJoinableTripsQuery query)
    {
        // Published trips a passenger can still board: pending and with free seats.
        // Optionally narrowed to a single route (the route-detail "join" flow).
        var trips = await context.Set<Trip>()
            .Where(t => t.Status == TripStatus.Pending && t.AvailableSeats > 0)
            .Where(t => query.RouteId == null || t.FkIdRoute == query.RouteId)
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
        return await BuildHistoryAsync(trips);
    }

    // Resolves names with bulk lookups (no N+1).
    private async Task<IEnumerable<TripHistoryView>> BuildHistoryAsync(List<Trip> trips)
    {
        if (trips.Count == 0) return Enumerable.Empty<TripHistoryView>();

        var userIds = trips.Select(t => t.FkIdUser).ToHashSet();
        var stopIds = trips.SelectMany(t => new[] { t.FkIdOriginStop, t.FkIdDestinationStop }).ToHashSet();
        var routeIds = trips.Select(t => t.FkIdRoute).ToHashSet();

        var users = await context.Set<User>()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        // The relevant driver is the one who owns the origin stop. Stop.FkIdDriver references the
        // driver entity id (drivers.id). Trip.FkIdDriver is usually null for passenger-created trips,
        // so we resolve the conductor from the origin stop's owner instead.
        var stopEntities = await context.Set<Stop>()
            .Where(s => stopIds.Contains(s.Id))
            .ToListAsync();
        var stops = stopEntities.ToDictionary(s => s.Id, s => s.Name);
        var stopDriverIds = stopEntities.ToDictionary(s => s.Id, s => s.FkIdDriver);

        // Two sources of conductor identity, in priority order:
        //   1. Trip.FkIdDriver  -> a driver who explicitly claimed this trip (option-2 flow).
        //   2. Stop owner       -> fallback for legacy/passenger-created trips with no claim.
        var stopOwnerIds = trips
            .Select(t => stopDriverIds.TryGetValue(t.FkIdOriginStop, out var did) ? did : 0)
            .Where(id => id > 0);
        var claimedDriverIds = trips
            .Where(t => t.FkIdDriver is > 0)
            .Select(t => t.FkIdDriver!.Value);
        var driverIds = stopOwnerIds.Concat(claimedDriverIds).ToHashSet();

        // Resolve driver names through the Driver bounded context ACL (respects BC boundaries).
        var driverNames = await driverContextFacade.FetchDriverNamesByDriverIdsAsync(driverIds);

        var routes = await context.Set<RouteAggregate>()
            .Where(r => routeIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToHashSetAsync();

        return trips.Select(t =>
        {
            // Prefer the driver who claimed the trip; fall back to the origin stop's owner.
            string driverName = "Sin conductor";
            if (t.FkIdDriver is > 0
                && driverNames.TryGetValue(t.FkIdDriver.Value, out var claimedName)
                && !string.IsNullOrWhiteSpace(claimedName))
                driverName = claimedName;
            else if (stopDriverIds.TryGetValue(t.FkIdOriginStop, out var stopDriverId)
                && driverNames.TryGetValue(stopDriverId, out var name)
                && !string.IsNullOrWhiteSpace(name))
                driverName = name;

            return new TripHistoryView(
                t.Id,
                routes.Contains(t.FkIdRoute) ? $"Ruta {t.FkIdRoute}" : "Ruta desconocida",
                stops.TryGetValue(t.FkIdOriginStop, out var origin) ? origin : "Origen desconocido",
                stops.TryGetValue(t.FkIdDestinationStop, out var dest) ? dest : "Destino desconocido",
                driverName,
                users.TryGetValue(t.FkIdUser, out var passenger) ? passenger : "Pasajero desconocido",
                t.StartTime,
                t.EndTime,
                t.Price,
                t.Status.ToString(),
                t.FkIdRoute,
                t.AvailableSeats);
        }).ToList();
    }
}
