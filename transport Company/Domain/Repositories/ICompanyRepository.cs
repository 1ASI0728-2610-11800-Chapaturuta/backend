using Frock_backend.shared.Domain.Repositories;
using Frock_backend.transport_Company.Domain.Model.Aggregates;


namespace Frock_backend.transport_Company.Domain.Repositories
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<Company?> FindByNameAsync(string name);
        Task<Company?> FindByFkIdUserAsync(int FkUserId);        
    }
}
