using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.stops.Domain.Services;

namespace Frock_backend.stops.Application.Internal.CommandServices
{
    /// <summary>
    ///     Stop command service.
    /// </summary>
    /// <remarks>
    ///     This class implements the basic operations for a Stop command service.
    /// </remarks>
    /// <param name="stopRepository">The instance of stopRepository</param>
    /// <param name="unitOfWork">The instance of UnitOfWork</param>
    /// See
    /// <see cref="IStopRepository">IStopRepository</see>
    /// ,
    /// <see cref="IUnitOfWork">IUnitOfWork</see>
    public class StopCommandService(IStopRepository stopRepository, IUnitOfWork unitOfWork) : IStopCommandService
    {
        public async Task<Stop?> Handle(CreateStopCommand command)
        {
            var existingStop =
                await stopRepository.FindByNameAndFkIdCompanyAsync(command.Name, command.FkIdCompany);
            // Note: The XML doc for IStopCommandService.Handle(CreateStopCommand) suggests an upsert behavior.
            // The current code throws if it exists. This is a discrepancy.
            // Keeping the throw behavior as per the current code for this example.
            if (existingStop != null)
            {
                // logger?.LogWarning("Create failed: Stop with name {StopName} already exists for Company {CompanyId}.", command.Name, command.FkIdCompany);
                // Consider a custom exception type for "already exists"
                throw new Exception($"Stop with name '{command.Name}' already exists for Company '{command.FkIdCompany}'.");
            }

            var newStop = new Stop(command);
            try
            {
                await stopRepository.AddAsync(newStop);
                await unitOfWork.CompleteAsync();
                return newStop;
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error creating stop with name {StopName} for District {DistrictId}.", command.Name, command.FkIdDistrict);
                return null; // Signal failure to the controller
            }
        }

        public async Task<Stop?> Handle(UpdateStopCommand command)
        {
            var stopToUpdate = await stopRepository.FindByIdAsync(command.Id);
            if (stopToUpdate == null)
            {
                // logger?.LogWarning("Update failed: Stop with ID {StopId} not found.", command.Id);
                return null; // Stop not found
            }

            // Apply changes from the command to the fetched entity
            stopToUpdate.Name = command.Name;
            stopToUpdate.GoogleMapsUrl = command.GoogleMapsUrl;
            stopToUpdate.ImageUrl = command.ImageUrl;
            stopToUpdate.Phone = command.Phone;
            stopToUpdate.FkIdCompany = command.FkIdCompany;
            stopToUpdate.Address = command.Address;
            stopToUpdate.Reference = command.Reference;
            stopToUpdate.FkIdDistrict = command.FkIdDistrict;
            stopToUpdate.Latitude = command.Latitude;
            stopToUpdate.Longitude = command.Longitude;

            try
            {
                stopRepository.Update(stopToUpdate); // Update the fetched and modified entity
                await unitOfWork.CompleteAsync();
                return stopToUpdate; // Return the updated entity
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error updating stop with ID {StopId}.", command.Id);
                return null; // Signal failure
            }
        }

        public async Task<Stop?> Handle(DeleteStopCommand command)
        {
            var stopToDelete = await stopRepository.FindByIdAsync(command.Id);
            if (stopToDelete == null)
            {
                // logger?.LogWarning("Delete failed: Stop with ID {StopId} not found.", command.Id);
                return null; // Stop not found
            }

            try
            {
                stopRepository.Remove(stopToDelete); // Remove the fetched entity
                await unitOfWork.CompleteAsync();
                return stopToDelete; // Return the (now conceptually deleted) entity as confirmation
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error deleting stop with ID {StopId}.", command.Id);
                return null; // Signal failure
            }
        }
    }
}
