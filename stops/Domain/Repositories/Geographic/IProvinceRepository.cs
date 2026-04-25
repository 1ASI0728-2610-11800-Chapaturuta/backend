using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Repositories.Geographic
{
    public interface IProvinceRepository : IBaseStringRepository<Province>
    {
        Task<IEnumerable<Province>> FindByFkIdRegionAsync(int fkIdRegion);
    }
}
