using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IRegionCommandService
    {
        /// <summary>
        ///     Handle the create region command.
        /// </summary>
        /// <remarks>
        ///     This method handles the create region command. It checks if the region already exists for the
        ///     given parameters. If it exists, it updates the existing region with the new values.
        ///     If it does not exist, it creates a new region and adds it to the database.
        /// </remarks>
        /// <param name="command">CreateRegionCommand command</param>
        /// <returns></returns>
        Task<Region?> Handle(CreateRegionCommand command);
    }
}
