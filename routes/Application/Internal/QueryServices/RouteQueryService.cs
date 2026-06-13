using Frock_backend.Driver.Interfaces.ACL;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Queries;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
using Frock_backend.Subscriptions.Interfaces.ACL;
namespace Frock_backend.routes.Application.Internal.QueryServices
{
    public class RouteQueryService(
        IRouteRepository routeRepository,
        IDriverContextFacade driverContextFacade,
        ISubscriptionsContextFacade subscriptionsContextFacade) : IRouteQueryService
    {
        public async Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesByFkDriverIdQuery query)
        {
            try
            {
                return await routeRepository.FindByDriverId(query.FkDriverId);
            }
            catch (Exception e)
            {

                throw new Exception($"Error retrieving routes for driver: {e.Message}", e);
            }
        }

        public async Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesQuery query)
        {
            try
            {
                var routes = await routeRepository.ListRoutes();
                return await OrderByPremiumFirstAsync(routes);
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving all routes: {e.Message}", e);
            }
        }

        public async Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesByFkDistrictIdQuery query)
        {
            try
            {
                var routes = await routeRepository.FindByDistrictId(query.FkDistrictId);
                return await OrderByPremiumFirstAsync(routes);
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving routes for district: {e.Message}", e);
            }
        }

        // Beneficio Premium: las rutas de conductores con plan Premium activo se muestran primero
        // en la búsqueda del pasajero. El conductor de la ruta se infiere de su primer paradero.
        // Se calcula el estado premium una vez por conductor (cache) y el orden es estable.
        private async Task<IEnumerable<RouteAggregate>> OrderByPremiumFirstAsync(List<RouteAggregate> routes)
        {
            var premiumByDriver = new Dictionary<int, bool>();

            int DriverIdOf(RouteAggregate r) => r.Stops.FirstOrDefault()?.Stop?.FkIdDriver ?? 0;

            foreach (var driverId in routes.Select(DriverIdOf).Where(d => d != 0).Distinct())
            {
                if (premiumByDriver.ContainsKey(driverId)) continue;
                var userId = await driverContextFacade.FetchUserIdByDriverIdAsync(driverId);
                premiumByDriver[driverId] = userId != null
                    && await subscriptionsContextFacade.HasActivePremiumPlanAsync(userId.Value);
            }

            bool IsPremium(RouteAggregate r) =>
                premiumByDriver.TryGetValue(DriverIdOf(r), out var p) && p;

            return routes.OrderByDescending(IsPremium).ToList();
        }

        public async Task<RouteAggregate?> Handle(GetRouteByIdQuery query)
        {
            try
            {
                return await routeRepository.FindByRouteId(query.Id);
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving route by ID: {e.Message}", e);
            }

        }
    }
}
