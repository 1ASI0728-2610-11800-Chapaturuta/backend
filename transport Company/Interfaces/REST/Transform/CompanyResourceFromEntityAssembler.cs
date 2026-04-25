using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.transport_Company.Interfaces.REST.Resources;


namespace Frock_backend.transport_Company.Interfaces.REST.Transform
{
    public static class CompanyResourceFromEntityAssembler
    {
        public static CompanyResource ToResourceFromEntity(Company entity) =>
            new CompanyResource(
                entity.Id,
                entity.Name,
                entity.LogoUrl,
                entity.FkIdUser
            );
    }
}
