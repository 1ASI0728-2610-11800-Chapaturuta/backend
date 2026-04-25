using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Queries;

namespace Frock_backend.stops.Domain.Services
{
    public interface IStopQueryService
    {
        /// <summary>
        ///     Handle the GetAllStopsByFkIdCompanyQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetAllStopsByFkIdCompanyQuery. It returns all the stops for the given
        ///     FkIdCompany.
        /// </remarks>
        /// <param name="query">The GetAllStopsByFkIdCompanyQuery query</param>
        /// <returns>An IEnumerable containing the Stops objects</returns>
        Task<IEnumerable<Stop>> Handle(GetAllStopsByFkIdCompanyQuery query);

        /// <summary>
        ///     Handle the GetAllStopsByFkIdLocalityQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetAllStopsByFkIdLocalityQuery. It returns the favorite source for the given
        ///     FkIdLocality
        /// </remarks>
        /// <param name="query">The GetAllStopsByFkIdLocalityQuery query</param>
        /// <returns>An IEnumerable containing the Stops objects</returns>
        /// 
        Task<IEnumerable<Stop>> Handle(GetAllStopsByFkIdDistrictQuery query);

        /// <summary>
        ///     Handle the GetStopByIdQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetStopByIdQuery. It returns the stop for the given Id.
        /// </remarks>
        /// <param name="query">The GetStopByIdQuery query</param>
        /// <returns>
        ///     The Stop object if found, or null otherwise
        /// </returns>
        Task<Stop?> Handle(GetStopByIdQuery query);

        Task<IEnumerable<Stop>> Handle(GetAllStopsQuery query);


        /// <summary>
        ///     Handle the GetStopByNameAndFkIdDistrictQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetStopByNameAndFkIdDistrictQuery. It returns the stop for the given name and fk_id_District.
        /// </remarks>
        /// <param name="query">The GetStopByNameAndFkIdDistrictQuery query</param>
        /// <returns>
        ///     The Stop object if found, or null otherwise
        /// </returns>
        Task<Stop?> Handle(GetStopByNameAndFkIdDistrictQuery query);

        /// <summary>
        /// Handle the GetStopByNameAndFkIdCompanyQuery.
        /// </summary> 
        /// <remarks>
        ///     This method handles the GetStopByNameAndFkIdCompanyQuery. It returns the stop for the given name and fk_id_Company.
        /// </remarks>
        /// <param name="query">The GetStopByNameAndFkIdCompanyQuery query</param>
        /// <returns>
        ///     The Stop object if found, or null otherwise
        /// </returns>
        Task<Stop?> Handle(GetStopByNameAndFkIdCompanyQuery query);

    }
}
