namespace Frock_backend.Trips.Domain.Model.Queries;

// Published, joinable trips for passengers (pending + free seats). RouteId narrows to one route.
public record GetJoinableTripsQuery(int? RouteId = null);
