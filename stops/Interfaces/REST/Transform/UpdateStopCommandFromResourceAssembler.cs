using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Interfaces.REST.Resources;

namespace Frock_backend.stops.Interfaces.REST.Transform
{
    public class UpdateStopCommandFromResourceAssembler
    {
        public static UpdateStopCommand ToCommandFromResource(UpdateStopResource resource)
        {
            return new UpdateStopCommand(
                resource.Id,
                resource.Name,
                resource.GoogleMapsUrl,
                resource.ImageUrl,
                resource.Phone,
                resource.FkIdCompany,
                resource.Address,
                resource.Reference,
                resource.FkIdDistrict,
                resource.Latitude,
                resource.Longitude
            );
        }
    }
}
