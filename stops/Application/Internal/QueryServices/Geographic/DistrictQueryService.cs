using Frock_backend.stops.Domain.Model.Aggregates.Geographic;
using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Repositories.Geographic;
using Frock_backend.stops.Domain.Services.Geographic;

namespace Frock_backend.stops.Application.Internal.QueryServices.Geographic
{
    public class DistrictQueryService(IDistrictRepository districtRepository) : IDistrictQueryService
    {
        public async Task<IEnumerable<District>> Handle(GetAllDistrictsQuery query)
        {
            return await districtRepository.ListAsync();
        }
        public async Task<District?> Handle(GetDistrictByIdQuery query)
        {
            return await districtRepository.FindByIdIntAsync(query.Id);
        }        
        public async Task<IEnumerable<District>> Handle(GetDistrictsByFkIdProvinceQuery query)
        {
            return await districtRepository.FindByFkIdProvinceAsync(query.FkIdProvince);
        }
    }
}
