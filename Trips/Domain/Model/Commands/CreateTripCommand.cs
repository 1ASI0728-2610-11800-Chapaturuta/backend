namespace Frock_backend.Trips.Domain.Model.Commands;

public record CreateTripCommand(int FkIdUser, int FkIdDriver, int FkIdRoute, int FkIdOriginStop, int FkIdDestinationStop, double? Price);
