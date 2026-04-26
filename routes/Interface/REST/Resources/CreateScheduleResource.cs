using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.routes.Interface.REST.Resources
{
    public record CreateScheduleResource(
        [property: SwaggerSchema("Day of the week in Spanish, e.g. Lunes, Martes, Miércoles")]
        string DayOfWeek,
        [property: SwaggerSchema("Service start time in HH:mm 24-hour format, e.g. 06:00")]
        string StartTime,
        [property: SwaggerSchema("Service end time in HH:mm 24-hour format, e.g. 22:00")]
        string EndTime,
        [property: SwaggerSchema("Whether this schedule is currently active")]
        bool Enabled
    );
}
