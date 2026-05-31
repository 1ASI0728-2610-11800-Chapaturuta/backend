using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.shared.Domain.Repositories;

namespace Frock_backend.routes.Domain.Repository
{
    public interface IRouteRepository:IBaseRepository<RouteAggregate>
    {
        Task<List<RouteAggregate>> FindByDriverId(int driverId);
        Task<List<RouteAggregate>> FindByDistrictId(int districtId);

        Task<List<RouteAggregate>> ListRoutes();

        Task<RouteAggregate?> FindByRouteId(int id);
    }
}
