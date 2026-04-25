namespace Frock_backend.routes.Domain.Model.Entities
{
    public class Schedule
    {
        public int Id { get; set; }
        public int RouteId { get; set; } // Foreign key to Route
        public string StartTime { get; set; } // Start time of the schedule
        public string EndTime { get; set; } // End time of the schedule
        public string DayOfWeek { get; set; } // Day of the week (e.g., "Monday", "Tuesday")
        public bool Enabled { get; set; } // Indicates if the schedule is enabled
        public Schedule(string startTime, string endTime, string dayOfWeek, bool enabled)
        {
            StartTime = startTime;
            EndTime = endTime;
            DayOfWeek = dayOfWeek;
            Enabled = enabled;
        }
    }
}
