using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Queries;

namespace Frock_backend.stops.Domain.Services
{
    public interface IStopQueryService
    {
        /// <summary>
        ///     Handle the GetAllStopsByFkIdDriverQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetAllStopsByFkIdDriverQuery. It returns all the stops for the given
        ///     FkIdDriver.
        /// </remarks>
        /// <param name="query">The GetAllStopsByFkIdDriverQuery query</param>
        /// <returns>An IEnumerable containing the Stops objects</returns>
        Task<IEnumerable<Stop>> Handle(GetAllStopsByFkIdDriverQuery query);

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
        /// Handle the GetStopByNameAndFkIdDriverQuery.
        /// </summary>
        /// <remarks>
        ///     This method handles the GetStopByNameAndFkIdDriverQuery. It returns the stop for the given name and fk_id_Driver.
        /// </remarks>
        /// <param name="query">The GetStopByNameAndFkIdDriverQuery query</param>
        /// <returns>
        ///     The Stop object if found, or null otherwise
        /// </returns>
        Task<Stop?> Handle(GetStopByNameAndFkIdDriverQuery query);

    }
}
