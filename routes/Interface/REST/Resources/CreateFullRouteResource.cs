using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.routes.Interface.REST.Resources
{
    public record CreateFullRouteResource
    (
        [property: SwaggerSchema("Bus frequency in minutes between consecutive departures")]
        int Frequency,
        [property: SwaggerSchema("Ticket price in Peruvian soles (S/.)")]
        double Price,
        [property: SwaggerSchema("Estimated trip duration in minutes from first to last stop")]
        int Duration,
        [property: SwaggerSchema("Ordered list of stop IDs that define the route path")]
        List<int> StopsIds,
        [property: SwaggerSchema("List of operating schedules (days and times) for this route")]
        List<CreateScheduleResource> Schedules
    );
}
