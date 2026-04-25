using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Frock_backend.stops.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Domain.Repositories;
using Frock_backend.stops.Infrastructure.Repositories;

namespace Frock_backend.routes.Application.Internal.CommandServices
{
    public class RouteCommandService(IRouteRepository routeRepository, IUnitOfWork unitOfWork):IRouteCommandService
    {
        public async Task<RouteAggregate?> Handle(CreateFullRouteCommand command)
        {

            var newRoute = new RouteAggregate(command);
            try
            {
                await routeRepository.AddAsync(newRoute);
                await unitOfWork.CompleteAsync();
                return newRoute;
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error creating stop with name {StopName} for locality {LocalityId}.", command.Name, command.FkIdLocality);
                return null; // Signal failure to the controller
            }
        }
        public async Task<RouteAggregate?> Handle(int idRoute,UpdateRouteCommand command)
        {
            Console.WriteLine($"Updating route with ID: {idRoute}");
            var route = await routeRepository.FindByIdAsync(idRoute);
            if (route == null)
            {
                return null; // Route not found
            }
            var updatedRoute = new RouteAggregate(command);
            try
            {
                routeRepository.Update(updatedRoute);
                await unitOfWork.CompleteAsync();
                return updatedRoute;
            }
            catch (Exception e)
            {
                // logger?.LogError(e, "Error updating route with ID {RouteId}.", command.IdRoute);
                return null; // Signal failure to the controller
            }
        }
        public async Task Handle(DeleteRouteCommand command)
        {
            var route = await routeRepository.FindByIdAsync(command.idRoute);
            if (route == null)
            {
                // logger?.LogWarning("Route with ID {RouteId} not found for deletion.", command.IdRoute);
                return; // Route not found, nothing to delete
            }
            try
            {
                routeRepository.Remove(route);
                await unitOfWork.CompleteAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<RouteAggregate?> ToggleAvailability(int idRoute)
        {
            var route = await routeRepository.FindByRouteId(idRoute);
            if (route == null) return null;

            route.IsActive = !route.IsActive;
            route.Status = route.IsActive ? "Active" : "Suspended";

            try
            {
                routeRepository.Update(route);
                await unitOfWork.CompleteAsync();
                return route;
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while toggling route availability: {e.Message}");
            }
        }
    }
}
