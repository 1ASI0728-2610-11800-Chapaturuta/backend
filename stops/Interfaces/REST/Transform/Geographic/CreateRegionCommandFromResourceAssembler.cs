using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Interfaces.REST.Resources.Geographic;

namespace Frock_backend.stops.Interfaces.REST.Transform.Geographic
{
    public class CreateRegionCommandFromResourceAssembler
    {
        public static CreateRegionCommand ToCommandFromResource(CreateRegionResource resource) =>
            new CreateRegionCommand(
                resource.Id,
                resource.Name
                );
    }
}
