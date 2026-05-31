namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record RouteDurationResource(int Id, int FkIdTariff, int FkIdRoute, int EstimatedMinutes);
