using Frock_backend.Trips.Domain.Model.Aggregates;
using Frock_backend.Trips.Interfaces.REST.Resources;

namespace Frock_backend.Trips.Interfaces.REST.Transform;

public static class TripResourceFromEntityAssembler
{
    public static TripResource ToResourceFromEntity(Trip entity) =>
        new TripResource(entity.Id, entity.FkIdUser, entity.FkIdDriver, entity.FkIdRoute, entity.FkIdOriginStop, entity.FkIdDestinationStop, entity.StartTime, entity.EndTime, entity.Price, entity.Status);
}
