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

public class TripQueryService(ITripRepository tripRepository, IReservationRepository reservationRepository, AppDbContext context, IDriverContextFacade driverContextFacade) : ITripQueryService
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
        // A passenger is linked to a published trip ONLY through a Reservation (Trip.FkIdUser holds
        // the publisher/driver-side user, not the reserving passenger). So the passenger's history
        // must union: trips they created (FkIdUser) + trips they reserved a seat on.
        var reservations = (await reservationRepository.FindByUserIdAsync(query.UserId)).ToList();

        // Latest reservation per trip — surfaces its status/seats on the history row.
        var myReservationsByTrip = reservations
            .GroupBy(r => r.FkIdTrip)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.ReservedAt).First());
        var reservedTripIds = myReservationsByTrip.Keys.ToHashSet();

        var trips = await context.Set<Trip>()
            .Where(t => t.FkIdUser == query.UserId || reservedTripIds.Contains(t.Id))
            .OrderByDescending(t => t.StartTime)
            .ToListAsync();
        return await BuildHistoryAsync(trips, myReservationsByTrip);
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
        // A driver may only see trip requests on routes they OWN. Route ownership = any of the
        // route's stops belongs to the driver (Stop.FkIdDriver). Resolve driver from the user.
        var driverId = await driverContextFacade.FetchDriverIdByUserIdAsync(query.UserId);
        if (driverId is null) return Enumerable.Empty<TripHistoryView>();

        var ownedRouteIds = await context.Set<RouteAggregate>()
            .Where(r => r.Stops.Any(rs => rs.Stop.FkIdDriver == driverId.Value))
            .Select(r => r.Id)
            .ToHashSetAsync();
        if (ownedRouteIds.Count == 0) return Enumerable.Empty<TripHistoryView>();

        var trips = await context.Set<Trip>()
            .Where(t => t.FkIdDriver == null && t.Status == TripStatus.Pending && ownedRouteIds.Contains(t.FkIdRoute))
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

    // Resolves names with bulk lookups (no N+1). When myReservationsByTrip is supplied, each row is
    // enriched with the querying passenger's reservation status/seats for that trip.
    private async Task<IEnumerable<TripHistoryView>> BuildHistoryAsync(
        List<Trip> trips,
        IReadOnlyDictionary<int, Reservation>? myReservationsByTrip = null)
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

            Reservation? myReservation = null;
            myReservationsByTrip?.TryGetValue(t.Id, out myReservation);

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
                t.AvailableSeats,
                myReservation?.Id,
                myReservation?.Status.ToString(),
                myReservation?.Seats);
        }).ToList();
    }
}
