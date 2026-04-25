using Frock_backend.stops.Domain.Model.Commands.Geographic;
using Frock_backend.stops.Domain.Model.Aggregates.Geographic;

namespace Frock_backend.stops.Domain.Services.Geographic
{
    public interface IProvinceCommandService
    {
        /// <summary>
        ///     Handle the create province command.
        /// </summary>
        /// <remarks>
        ///     This method handles the create province command. It checks if the province already exists for the
        ///     given parameters. If it exists, it updates the existing province with the new values.
        ///     If it does not exist, it creates a new province and adds it to the database.
        /// </remarks>
        /// <param name="command">CreateProvinceCommand command</param>
        /// <returns></returns>
        Task<Province?> Handle(CreateProvinceCommand command);
    }
}
