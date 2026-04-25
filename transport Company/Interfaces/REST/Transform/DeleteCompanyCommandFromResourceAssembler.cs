using Frock_backend.transport_Company.Domain.Model.Commands;
using Frock_backend.transport_Company.Interfaces.REST.Resources;

namespace Frock_backend.transport_Company.Interfaces.REST.Transform
{
    public class DeleteCompanyCommandFromResourceAssembler
    {
        public static DeleteCompanyCommand ToCommandFromResource(DeleteCompanyResource resource)
        {
            return new DeleteCompanyCommand(resource.Id);
        }
    }
}
