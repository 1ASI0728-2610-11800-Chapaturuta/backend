using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IDistrictQueryService
    {
        Task<IEnumerable<District>> Handle(GetAllDistrictsQuery query);

        /// <summary>
        ///     Handle the GetDistrictsByFkIdProvinceQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetDistrictsByFkIdProvinceQuery. It returns all the districts for the given
        ///     FkIdProvince.
        /// </remarks>
        /// <param name="query">The GetDistrictsByFkIdProvinceQuery query</param>
        /// <returns>An IEnumerable containing the District objects</returns>
        Task<IEnumerable<District>> Handle(GetDistrictsByFkIdProvinceQuery query);
        /// <summary>
        ///     Handle the GetDistrictByIdQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetDistrictByIdQuery. It returns the district for the given Id.
        /// </remarks>
        /// <param name="query">The GetDistrictByIdQuery query</param>
        /// <returns>The District object if found, or null otherwise</returns>
        Task<District?> Handle(GetDistrictByIdQuery query);

    }
}
