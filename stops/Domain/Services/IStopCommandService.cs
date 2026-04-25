using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Commands;

namespace Frock_backend.stops.Domain.Services
{
    public interface IStopCommandService
    {        /// <summary>
             ///     Handle the create favorite source command.
             /// </summary>
             /// <remarks>
             ///     This method handles the create stop command. It checks if the stop already exists for the
             ///     given parameters. If it exists, it updates the existing stop with the new values.
             ///     If it does not exist, it creates a new stop and adds it to the database.
             /// </remarks>
             /// <param name="command">CreateStopCommand command</param>
             /// <returns></returns>
             /// <exception cref="Exception"></exception>
        Task<Stop?> Handle(CreateStopCommand command);
        Task<Stop?> Handle(UpdateStopCommand command);
        Task<Stop?> Handle(DeleteStopCommand command);
    }
}
