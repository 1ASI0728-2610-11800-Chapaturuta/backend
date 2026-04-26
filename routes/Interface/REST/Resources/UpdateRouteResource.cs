using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.routes.Interface.REST.Resources
{
    public record UpdateRouteResource
    (
        [property: SwaggerSchema("Updated ticket price in Peruvian soles (S/.)")]
        double Price,
        [property: SwaggerSchema("Updated estimated trip duration in minutes")]
        int Duration,
        [property: SwaggerSchema("Updated bus frequency in minutes between consecutive departures")]
        int Frequency,
        [property: SwaggerSchema("Updated ordered list of stop IDs that define the route path")]
        List<int> StopsIds,
        [property: SwaggerSchema("Updated list of operating schedules for this route")]
        List<ScheduleResource> Schedules
    );
}
