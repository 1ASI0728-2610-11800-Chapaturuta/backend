namespace Frock_backend.routes.Interface.REST.Resources
{
    public record ScheduleResource
    (
        string startTime, // Time in HH:mm format
        string endTime,
        string dayOfWeek, // Day of the week, e.g., "Monday", "Tuesday", etc.
        bool enabled // Indicates if the schedule is enabled or not
        );
}
