using Frock_backend.routes.Domain.Model.ValueObjects;

namespace Frock_backend.routes.Domain.Service;

public interface IOsrmRoutingService
{
    Task<OsrmRouteResult> RouteAsync(IEnumerable<Coordinate> waypoints);
    Task<IEnumerable<double>> TableAsync(Coordinate source, IEnumerable<Coordinate> destinations);
    Task<Coordinate?> NearestAsync(Coordinate point);
}
