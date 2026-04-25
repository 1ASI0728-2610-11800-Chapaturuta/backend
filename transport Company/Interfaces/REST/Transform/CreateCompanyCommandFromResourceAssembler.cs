using Frock_backend.transport_Company.Domain.Model.Commands;
using Frock_backend.transport_Company.Interfaces.REST.Resources;


namespace Frock_backend.transport_Company.Interfaces.REST.Transform
{
    public class CreateCompanyCommandFromResourceAssembler
    {
        public static CreateCompanyCommand ToCommandFromResource(CreateCompanyResource resource) =>
            new CreateCompanyCommand(
                resource.Name,
                resource.LogoUrl,
                resource.FkIdUser
            );
    }
}
