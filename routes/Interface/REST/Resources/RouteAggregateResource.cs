
namespace Frock_backend.routes.Interface.REST.Resources
{
    public record RouteAggregateResource
    (
        int id,
        double price,
        int frequency,
        int duration,
        bool isActive,
        string status,
        decimal? distanceMeters,
        int? durationSeconds,
        string? geometry,
        List<StopInRoutesResource> stops,
        List<ScheduleResource> schedules
    );
}
