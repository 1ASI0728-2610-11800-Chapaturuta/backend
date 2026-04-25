using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;

namespace Frock_backend.stops.Application.Internal.QueryServices.Geographic
{
    public class ProvinceQueryService(IProvinceRepository provinceRepository) : IProvinceQueryService
    {
        public async Task<IEnumerable<Province>> Handle(GetAllProvincesQuery query)
        {
            return await provinceRepository.ListAsync();
        }
        public async Task<Province?> Handle(GetProvinceByIdQuery query)
        {
            return await provinceRepository.FindByIdIntAsync(query.Id);
        }

        public async Task<IEnumerable<Province>> Handle(GetProvincesByFkIdRegionQuery query)
        {
            return await provinceRepository.FindByFkIdRegionAsync(query.FkIdRegion);
        }
    }
}
