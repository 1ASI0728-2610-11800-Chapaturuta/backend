using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Interfaces.REST.Resources;

namespace Frock_backend.Trips.Interfaces.REST.Transform;

public static class TripHistoryResourceFromViewAssembler
{
    public static TripHistoryResource ToResourceFromView(TripHistoryView view) =>
        new TripHistoryResource(
            view.Id,
            view.RouteName,
            view.OriginName,
            view.DestinationName,
            view.DriverName,
            view.PassengerName,
            view.StartTime,
            view.EndTime,
            view.Price,
            view.Status);
}
