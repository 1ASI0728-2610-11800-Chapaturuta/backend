namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record TripResource(int Id, int FkIdUser, int? FkIdDriver, int FkIdRoute, int FkIdOriginStop, int FkIdDestinationStop, DateTime StartTime, DateTime? EndTime, decimal? Price, string Status, int AvailableSeats);
