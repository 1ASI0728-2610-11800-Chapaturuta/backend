using Frock_backend.routes.Domain.Model.Entities;

namespace Frock_backend.routes.Domain.Model.Commands
{
    public record UpdateRouteCommand
    (
        double Price,
        int Duration, // in minutes
        int Frequency, // in minutes
        List<int> StopsIds,
        List<Schedule> Schedules
    );    
}
