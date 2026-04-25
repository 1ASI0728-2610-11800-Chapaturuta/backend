using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public class CreateProvinceCommandFromResourceAssembler
    {
        public static CreateProvinceCommand ToCommandFromResource(CreateProvinceResource resource) =>
            new CreateProvinceCommand(
                resource.Id,
                resource.Name,
                resource.FkIdRegion
            );
    }
}
