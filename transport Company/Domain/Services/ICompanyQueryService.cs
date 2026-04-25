using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.transport_Company.Domain.Model.Queries;

namespace Frock_backend.transport_Company.Domain.Services
{
    public interface ICompanyQueryService
    {
        Task<IEnumerable<Company>> Handle(GetAllCompaniesQuery query);

        Task<Company?> Handle(GetCompanyByIdQuery query);

        Task<Company?> Handle(GetCompanyByNameQuery query);

        Task<Company?> Handle(GetCompanyByFkIdUserQuery query);
        
    }
}
