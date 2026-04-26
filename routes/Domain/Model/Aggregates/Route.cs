using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Model.Entities;
using Frock_backend.stops.Domain.Model.Aggregates;

namespace Frock_backend.routes.Domain.Model.Aggregates
{
    public class RouteAggregate
    {
        public int Id { get; set; }
        public double Price { get; set; }
        public int Duration { get; set; } // in minutes
        public int Frequency { get; set; } // in minutes
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Active"; // Draft, Active, Suspended, Archived
        public decimal? DistanceMeters { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Geometry { get; set; }
        public List<Schedule> Schedules = new();
        public List<RoutesStops> Stops = new();

        public void SetOsrmData(double distanceMeters, double durationSeconds, string geometry)
        {
            DistanceMeters = (decimal)distanceMeters;
            DurationSeconds = (int)durationSeconds;
            Geometry = geometry;
        }
        public RouteAggregate(double Price, int Duration, int Frequency)
        {
            this.Price = Price;
            this.Duration = Duration;
            this.Frequency = Frequency;
        }
        // 3) Método para añadir un horario
        public void AddSchedule(string start, string end, string dayOfWeek, bool enabled)
        {
            Schedules.Add(new Schedule( start, end, dayOfWeek, enabled));
        }
        public RouteAggregate(CreateFullRouteCommand cm)
        {
            this.Price = cm.Price;
            this.Duration = cm.Duration;
            this.Frequency = cm.Frequency;
            foreach (var stopId in cm.StopsIds)
            {
                // Assuming RoutesStops is a value object that holds the stop ID
                var routeStop = new RoutesStops(stopId);
                // Here you would typically add this to a collection of stops in the Route aggregate
                this.Stops.Add(routeStop);
            }
            foreach (var schedule in cm.Schedules)
            {
                this.AddSchedule(schedule.StartTime, schedule.EndTime, schedule.DayOfWeek, schedule.Enabled);
            }
        }
        public RouteAggregate(UpdateRouteCommand cm)
        {
            this.Price = cm.Price;
            this.Duration = cm.Duration;
            this.Frequency = cm.Frequency;
            foreach (var stopId in cm.StopsIds)
            {
                // Assuming RoutesStops is a value object that holds the stop ID
                var routeStop = new RoutesStops(stopId);
                // Here you would typically add this to a collection of stops in the Route aggregate
                this.Stops.Add(routeStop);
            }
            foreach (var schedule in cm.Schedules)
            {
                this.AddSchedule(schedule.StartTime, schedule.EndTime, schedule.DayOfWeek, schedule.Enabled);
            }
        }
        public RouteAggregate(DeleteRouteCommand cm)
        {
            this.Id = cm.idRoute;
        }
    }
}
