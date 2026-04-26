using Frock_backend.routes.Domain.Exceptions;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Model.ValueObjects;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.shared.Domain.Repositories;
using Frock_backend.stops.Domain.Repositories;

namespace Frock_backend.routes.Application.Internal.CommandServices
{
    public class RouteCommandService(
        IRouteRepository routeRepository,
        IUnitOfWork unitOfWork,
        IStopRepository stopRepository,
        IOsrmRoutingService osrmRoutingService) : IRouteCommandService
    {
        public async Task<RouteAggregate?> Handle(CreateFullRouteCommand command)
        {
            var newRoute = new RouteAggregate(command);

            var waypoints = await GetWaypointsForStops(command.StopsIds);
            if (waypoints.Count >= 2)
            {
                var osrmResult = await osrmRoutingService.RouteAsync(waypoints);
                newRoute.SetOsrmData(osrmResult.DistanceMeters, osrmResult.DurationSeconds, osrmResult.Geometry);
            }

            try
            {
                await routeRepository.AddAsync(newRoute);
                await unitOfWork.CompleteAsync();
                return newRoute;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<RouteAggregate?> Handle(int idRoute, UpdateRouteCommand command)
        {
            var route = await routeRepository.FindByIdAsync(idRoute);
            if (route == null) return null;

            var updatedRoute = new RouteAggregate(command);
            updatedRoute.Id = idRoute;

            var waypoints = await GetWaypointsForStops(command.StopsIds);
            if (waypoints.Count >= 2)
            {
                var osrmResult = await osrmRoutingService.RouteAsync(waypoints);
                updatedRoute.SetOsrmData(osrmResult.DistanceMeters, osrmResult.DurationSeconds, osrmResult.Geometry);
            }

            try
            {
                routeRepository.Update(updatedRoute);
                await unitOfWork.CompleteAsync();
                return updatedRoute;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task Handle(DeleteRouteCommand command)
        {
            var route = await routeRepository.FindByIdAsync(command.idRoute);
            if (route == null) return;
            routeRepository.Remove(route);
            await unitOfWork.CompleteAsync();
        }

        public async Task<RouteAggregate?> ToggleAvailability(int idRoute)
        {
            var route = await routeRepository.FindByRouteId(idRoute);
            if (route == null) return null;

            route.IsActive = !route.IsActive;
            route.Status = route.IsActive ? "Active" : "Suspended";

            routeRepository.Update(route);
            await unitOfWork.CompleteAsync();
            return route;
        }

        private async Task<List<Coordinate>> GetWaypointsForStops(List<int> stopIds)
        {
            var waypoints = new List<Coordinate>();
            foreach (var id in stopIds)
            {
                var stop = await stopRepository.FindByIdAsync(id);
                if (stop?.Latitude.HasValue == true && stop.Longitude.HasValue)
                    waypoints.Add(new Coordinate(stop.Latitude.Value, stop.Longitude.Value));
            }
            return waypoints;
        }
    }
}
