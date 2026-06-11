using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Repositories;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Configuration;
using Frock_backend.IAM.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Aggregates;
using DriverAggregate = Frock_backend.Driver.Domain.Model.Aggregates.Driver;
using Microsoft.EntityFrameworkCore;

namespace Frock_backend.Trips.Application.Internal.QueryServices;

public class TripQueryService(ITripRepository tripRepository, AppDbContext context) : ITripQueryService
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

    // Resolves names with bulk lookups (5 queries total, no N+1).
    private async Task<IEnumerable<TripHistoryView>> BuildHistoryAsync(List<Trip> trips)
    {
        if (trips.Count == 0) return Enumerable.Empty<TripHistoryView>();

        var userIds = trips.Select(t => t.FkIdUser).ToHashSet();
        var driverUserIds = trips.Where(t => t.FkIdDriver.HasValue).Select(t => t.FkIdDriver!.Value).ToHashSet();
        var stopIds = trips.SelectMany(t => new[] { t.FkIdOriginStop, t.FkIdDestinationStop }).ToHashSet();
        var routeIds = trips.Select(t => t.FkIdRoute).ToHashSet();

        var users = await context.Set<User>()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        // Trip.FkIdDriver references the driver's IAM user id; Driver.FkIdUser links back to it.
        var drivers = await context.Set<DriverAggregate>()
            .Where(d => driverUserIds.Contains(d.FkIdUser))
            .ToDictionaryAsync(d => d.FkIdUser, d => (d.FirstName + " " + d.LastName).Trim());

        var stops = await context.Set<Stop>()
            .Where(s => stopIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name);

        var routes = await context.Set<RouteAggregate>()
            .Where(r => routeIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToHashSetAsync();

        return trips.Select(t => new TripHistoryView(
            t.Id,
            routes.Contains(t.FkIdRoute) ? $"Ruta {t.FkIdRoute}" : "Ruta desconocida",
            stops.TryGetValue(t.FkIdOriginStop, out var origin) ? origin : "Origen desconocido",
            stops.TryGetValue(t.FkIdDestinationStop, out var dest) ? dest : "Destino desconocido",
            t.FkIdDriver.HasValue && drivers.TryGetValue(t.FkIdDriver.Value, out var driverName) ? driverName : "Sin conductor",
            users.TryGetValue(t.FkIdUser, out var passenger) ? passenger : "Pasajero desconocido",
            t.StartTime,
            t.EndTime,
            t.Price,
            t.Status.ToString())).ToList();
    }
}
