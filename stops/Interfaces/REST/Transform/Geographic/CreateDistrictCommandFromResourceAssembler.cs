using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public class CreateDistrictCommandFromResourceAssembler
    {
        public static CreateDistrictCommand ToCommandFromResource(CreateDistrictResource resource) =>
            new CreateDistrictCommand(
                resource.Id,
                resource.Name,
                resource.FkIdProvince
            );
    }
}
