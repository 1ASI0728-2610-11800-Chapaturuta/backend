using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Interfaces.REST.Resources;

namespace Frock_backend.stops.Interfaces.REST.Transform
{
    /// <summary>
    /// Assembles a CreateStopCommand from a CreateStopResource. 
    /// </summary>
    /// <param name="resource">The CreateStopResource resource</param>
    /// <returns>
    /// A CreateStopCommand assembled from the CreateStopResource
    /// </returns>
    public class CreateStopCommandFromResourceAssembler
    {
        public static CreateStopCommand ToCommandFromResource(CreateStopResource resource) =>
            new CreateStopCommand(
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
