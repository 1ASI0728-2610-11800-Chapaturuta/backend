namespace Frock_backend.routes.Interface.REST.Resources
{
    public record CreateScheduleResource(
        string DayOfWeek,
        string StartTime,
        string EndTime,
        bool Enabled
    );
}
