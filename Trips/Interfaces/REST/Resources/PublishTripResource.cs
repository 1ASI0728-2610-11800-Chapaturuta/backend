namespace Frock_backend.Trips.Interfaces.REST.Resources;

// A driver publishes a shared trip for a route with a fixed seat capacity.
public record PublishTripResource(
    int FkIdUser,
    int FkIdDriver,
    int FkIdRoute,
    int FkIdOriginStop,
    int FkIdDestinationStop,
    decimal? Price,
    int Seats,
    DateTime? StartTime = null);
