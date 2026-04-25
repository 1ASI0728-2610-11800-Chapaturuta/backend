using Frock_backend.routes.Domain.Model.Queries;
using Frock_backend.routes.Domain.Model.Aggregates;
namespace Frock_backend.routes.Domain.Service
{
    public interface IRouteQueryService
    {
        Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesByFkCompanyIdQuery query);

        Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesQuery query);

        Task<IEnumerable<RouteAggregate>> Handle(GetAllRoutesByFkDistrictIdQuery query);

        Task<RouteAggregate?> Handle(GetRouteByIdQuery query);
    }
}
