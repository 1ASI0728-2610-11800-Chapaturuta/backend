using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public static class ProvinceResourceFromEntityAssembler
    {
        public static ProvinceResource ToResourceFromEntity(Province entity) =>
            new ProvinceResource(
                entity.Id,
                entity.Name,
                entity.FkIdRegion
            );
    }
}
