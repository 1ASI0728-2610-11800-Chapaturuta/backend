using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;

namespace Frock_backend.stops.Domain.Repositories
{
    public interface IStopRepository : IBaseRepository<Stop>
    {
        Task<IEnumerable<Stop>> FindByFkIdDriverAsync(int fkIdDriver);
        Task<IEnumerable<Stop>> FindByFkIdDistrictAsync(int fkIdDistrict);
        Task<Stop?> FindByNameAndFkIdDistrictAsync(string name, int fkIdDistrict);

        Task<Stop?> FindByNameAndFkIdDriverAsync(string name, int fkIdDriver);
    }
}
