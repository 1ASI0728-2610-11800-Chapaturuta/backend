namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record CreateTripResource(int FkIdUser, int FkIdDriver, int FkIdRoute, int FkIdOriginStop, int FkIdDestinationStop, double? Price);
