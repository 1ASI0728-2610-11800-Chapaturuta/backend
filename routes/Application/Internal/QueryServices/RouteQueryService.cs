using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Model.Queries;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.routes.Domain.Service;
using Frock_backend.shared.Infrastructure.Persistences.EFC.Repositories;
namespace Frock_backend.routes.Application.Internal.QueryServices
{
    public class RouteQueryService(IRouteRepository routeRepository) : IRouteQueryService
    {
        public async Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesByFkCompanyIdQuery query)
        {
            try
            {
                return await routeRepository.FindByCompanyId(query.FkCompanyId);
            }
            catch (Exception e)
            {

                throw new Exception($"Error retrieving routes for company: {e.Message}", e);
            }
        }

        public async Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesQuery query)
        {
            try
            {
                return await routeRepository.ListRoutes();
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
                return await routeRepository.FindByDistrictId(query.FkDistrictId);
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving routes for district: {e.Message}", e);
            }
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
