using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Interfaces.REST.Resources;

namespace Frock_backend.stops.Interfaces.REST.Transform
{
    public class DeleteStopCommandFromResourceAssembler
    {
        public static DeleteStopCommand ToCommandFromResource(DeleteStopResource resource)
        {
            return new DeleteStopCommand(resource.Id);
        }

    }
}
