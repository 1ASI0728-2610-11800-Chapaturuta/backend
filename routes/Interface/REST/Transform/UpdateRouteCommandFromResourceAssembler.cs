using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Model.Entities;
using Frock_backend.routes.Interface.REST.Resources;

namespace Frock_backend.routes.Interface.REST.Transform
{
    public class UpdateRouteCommandFromResourceAssembler
    {
        public static UpdateRouteCommand toCommandFromResource(UpdateRouteResource resource)
        {
            return new UpdateRouteCommand
            (
                Price: resource.Price,
                Duration: resource.Duration,
                Frequency: resource.Frequency,
                StopsIds: resource.StopsIds,
                Schedules: resource.Schedules.Select(schedule => new Schedule
                (
                    startTime: schedule.startTime,
                    endTime: schedule.endTime,
                    dayOfWeek: schedule.dayOfWeek,
                    enabled: schedule.enabled
                )).ToList()
            );
        }
    }
}
