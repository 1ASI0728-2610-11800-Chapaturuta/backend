using Frock_backend.transport_Company.Domain.Model.Aggregates;
using Frock_backend.transport_Company.Domain.Model.Queries;
using Frock_backend.transport_Company.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Services;

namespace Frock_backend.transport_Company.Application.Internal.QueryServices
{
    public class CompanyQueryService(ICompanyRepository companyRepository) : ICompanyQueryService
    {
        public async Task<IEnumerable<Company>> Handle(GetAllCompaniesQuery query)
        {
            return await companyRepository.ListAsync();
        }
        public async Task<Company?> Handle(GetCompanyByIdQuery query)
        {
            return await companyRepository.FindByIdAsync(query.Id);
        }
        public async Task<Company?> Handle(GetCompanyByNameQuery query)
        {
            return await companyRepository.FindByNameAsync(query.Name);
        }

        public async Task<Company?> Handle(GetCompanyByFkIdUserQuery query)
        {
            return await companyRepository.FindByFkIdUserAsync(query.FkIdUser);
        }
    }
}
