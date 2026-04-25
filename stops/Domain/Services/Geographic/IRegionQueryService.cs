using Frock_backend.stops.Domain.Model.Queries.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IRegionQueryService
    {
        /// <summary>
        ///     Handle the GetAllRegionsQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetAllRegionsQuery. It returns all the regions
        /// </remarks>
        /// <param name="query">The GetAllRegionsQuery query</param>
        /// <returns>An IEnumerable containing the Region objects</returns>
        Task<IEnumerable<Region>> Handle(GetAllRegionsQuery query);
        /// <summary>
        ///     Handle the GetRegionByIdQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetRegionByIdQuery. It returns the region for the given Id.
        /// </remarks>
        /// <param name="query">The GetRegionByIdQuery query</param>
        /// <returns>
        ///     The Region object if found, or null otherwise
        /// </returns>
        Task<Region?> Handle(GetRegionByIdQuery query);
    }
}
