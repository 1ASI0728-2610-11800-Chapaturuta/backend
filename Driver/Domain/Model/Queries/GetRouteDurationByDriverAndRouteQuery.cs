namespace Frock_backend.Driver.Domain.Model.Queries;

public record GetRouteDurationByDriverAndRouteQuery(int FkIdDriver, int FkIdRoute);
