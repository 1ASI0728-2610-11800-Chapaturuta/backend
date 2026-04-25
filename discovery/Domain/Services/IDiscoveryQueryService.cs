using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates;

namespace Frock_backend.Discovery.Domain.Services;

public interface IDiscoveryQueryService
{
    Task<IEnumerable<RouteAggregate>> Handle(SearchRoutesQuery query);
    Task<IEnumerable<Stop>> Handle(GetNearbyStopsQuery query);
    Task<IEnumerable<RouteAggregate>> Handle(GetPopularRoutesQuery query);
    Task<object> Handle(GetDemandAnalyticsQuery query);
}
