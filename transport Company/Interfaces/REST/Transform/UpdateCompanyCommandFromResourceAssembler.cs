using Frock_backend.transport_Company.Domain.Model.Commands;
using Frock_backend.transport_Company.Interfaces.REST.Resources;


namespace Frock_backend.transport_Company.Interfaces.REST.Transform
{
    public class UpdateCompanyCommandFromResourceAssembler
    {
        public static UpdateCompanyCommand ToCommandFromResource(UpdateCompanyResource resource)
        {
            return new UpdateCompanyCommand(
                resource.Id,
                resource.Name,
                resource.LogoUrl,
                resource.FkIdUser
            );
        }
    }
}
