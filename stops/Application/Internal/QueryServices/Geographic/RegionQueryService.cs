using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;

namespace Frock_backend.stops.Application.Internal.QueryServices.Geographic
{
    public class RegionQueryService(IRegionRepository regionRepository) : IRegionQueryService
    {
        public async Task<IEnumerable<Region>> Handle(GetAllRegionsQuery query)
        {
            return await regionRepository.ListAsync();
        }
        public async Task<Region?> Handle(GetRegionByIdQuery query)
        {
            return await regionRepository.FindByIdIntAsync(query.Id);
        }
    }
}
