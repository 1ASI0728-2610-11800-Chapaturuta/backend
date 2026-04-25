using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.stops.Domain.Model.Aggregates;
namespace Frock_backend.routes.Domain.Model.Entities
{
    public class RoutesStops
    {
        public int Id { get; set; } // Unique identifier for the RoutesStops entity
        public int FkStopId { get; set; } // Foreign key to Route
        public int FKRouteId { get; set; } // Foreign key to Stop
        public RouteAggregate Route { get; set; } // Navigation property to Route
        public Stop Stop { get; set; } // Navigation property to Stop
        public RoutesStops(int FkStopId)
        {
            this.FkStopId = FkStopId;
        }
        public RoutesStops(int stopId, string name, string address, int fkCompanyId, int fkDistrictId)
        {
            this.Stop = new Stop(stopId, name, address, fkCompanyId, fkDistrictId);
        }
    }
}
