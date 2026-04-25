using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Repositories.Geographic
{
    public interface IDistrictRepository : IBaseStringRepository<District>
    {
        Task<IEnumerable<District>> FindByFkIdProvinceAsync(int fkIdProvince);
    }
}
