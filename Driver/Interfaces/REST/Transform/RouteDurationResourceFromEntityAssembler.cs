using Frock_backend.Driver.Domain.Model.Aggregates;
using Frock_backend.Driver.Interfaces.REST.Resources;

namespace Frock_backend.Driver.Interfaces.REST.Transform;

public static class RouteDurationResourceFromEntityAssembler
{
    public static RouteDurationResource ToResourceFromEntity(RouteDuration entity) =>
        new RouteDurationResource(entity.Id, entity.FkIdTariff, entity.FkIdRoute, entity.EstimatedMinutes);
}
