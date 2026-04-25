using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IProvinceQueryService
    {
        Task<IEnumerable<Province>> Handle(GetAllProvincesQuery query);

        /// <summary>
        ///     Handle the GetProvincesByFkIdRegionQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetProvincesByFkIdRegionQuery. It returns all provinces for the given region ID.
        /// </remarks>
        /// <param name="query">The GetProvincesByFkIdRegionQuery query</param>
        /// <returns>An IEnumerable containing the Province objects for the specified region</returns>
        Task<IEnumerable<Province>> Handle(GetProvincesByFkIdRegionQuery query);

        Task<Province?> Handle(GetProvinceByIdQuery query);
    }
}
