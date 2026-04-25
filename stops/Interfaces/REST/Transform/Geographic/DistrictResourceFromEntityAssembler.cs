using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public static class DistrictResourceFromEntityAssembler
    {
        public static DistrictResource ToResourceFromEntity(District entity) =>
            new DistrictResource(
                entity.Id,
                entity.Name,
                entity.FkIdProvince
            );
    }
}
