using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.routes.Domain.Model.Aggregates;
namespace Frock_backend.routes.Interface.REST.Transform
{
    public class RouteAggregateResourceFromResourceAssembler
    {
        public static RouteAggregateResource ToResourceFromEntity(RouteAggregate routeAggregate) =>
            new RouteAggregateResource(
                routeAggregate.Id,
                routeAggregate.Price,
                routeAggregate.Frequency,
                routeAggregate.Duration,
                routeAggregate.IsActive,
                routeAggregate.Status,
                routeAggregate.DistanceMeters,
                routeAggregate.DurationSeconds,
                routeAggregate.Geometry,
                routeAggregate.Stops.Select(stop => new StopInRoutesResource(stop.Id, stop.Stop.Name, stop.Stop.ImageUrl, stop.Stop.Address, stop.Stop.FkIdCompany, stop.Stop.FkIdDistrict)).ToList(),
                routeAggregate.Schedules.Select(schedule => new ScheduleResource(schedule.StartTime, schedule.EndTime, schedule.DayOfWeek, schedule.Enabled)).ToList()
            );
   
    }
}
