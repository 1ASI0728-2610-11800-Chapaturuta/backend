using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.shared.Domain.Geo;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.stops.Domain.Services;
using Frock_backend.Subscriptions.Interfaces.ACL;

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
    public class StopCommandService(
        IStopRepository stopRepository,
        IUnitOfWork unitOfWork,
        IDriverContextFacade driverContextFacade,
        ISubscriptionsContextFacade subscriptionsContextFacade) : IStopCommandService
    {
        private const int BasicMaxStops = 30;

        public async Task<Stop?> Handle(CreateStopCommand command)
        {
            if (!PeruBounds.Contains(command.Latitude, command.Longitude))
                throw new ArgumentException("La ubicación del paradero debe estar dentro de Perú.");

            await EnforceStopLimitAsync(command.FkIdDriver);

            var existingStop =
                await stopRepository.FindByNameAndFkIdDriverAsync(command.Name, command.FkIdDriver);
            // Note: The XML doc for IStopCommandService.Handle(CreateStopCommand) suggests an upsert behavior.
            // The current code throws if it exists. This is a discrepancy.
            // Keeping the throw behavior as per the current code for this example.
            if (existingStop != null)
            {
                throw new InvalidOperationException($"Ya tienes un paradero con el nombre '{command.Name}'.");
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

        // El plan Básico limita los paraderos por conductor; Premium es ilimitado.
        private async Task EnforceStopLimitAsync(int driverId)
        {
            var userId = await driverContextFacade.FetchUserIdByDriverIdAsync(driverId);
            if (userId == null) return; // sin conductor asociado, no se aplica el límite

            var isPremium = await subscriptionsContextFacade.HasActivePremiumPlanAsync(userId.Value);
            if (isPremium) return;

            var count = await stopRepository.CountByFkIdDriverAsync(driverId);
            if (count >= BasicMaxStops)
                throw new InvalidOperationException(
                    $"Alcanzaste el límite de {BasicMaxStops} paraderos del plan Básico. Pasa a Premium para paraderos ilimitados.");
        }

        public async Task<Stop?> Handle(UpdateStopCommand command)
        {
            if (!PeruBounds.Contains(command.Latitude, command.Longitude))
                throw new ArgumentException("La ubicación del paradero debe estar dentro de Perú.");

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
            stopToUpdate.FkIdDriver = command.FkIdDriver;
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
