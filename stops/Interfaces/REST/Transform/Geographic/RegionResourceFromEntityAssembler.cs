using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public static class RegionResourceFromEntityAssembler
    {
        public static RegionResource ToResourceFromEntity(Region entity) =>
            new RegionResource(
                entity.Id,
                entity.Name
            );
    }
}
