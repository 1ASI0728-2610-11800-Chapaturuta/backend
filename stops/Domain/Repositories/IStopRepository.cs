using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;

namespace Frock_backend.stops.Domain.Repositories
{
    public interface IStopRepository : IBaseRepository<Stop>
    {
        Task<IEnumerable<Stop>> FindByFkIdCompanyAsync(int fkIdCompany);
        Task<IEnumerable<Stop>> FindByFkIdDistrictAsync(int fkIdDistrict);
        Task<Stop?> FindByNameAndFkIdDistrictAsync(string name, int fkIdDistrict);

        Task<Stop?> FindByNameAndFkIdCompanyAsync(string name, int fkIdCompany);
    }
}
