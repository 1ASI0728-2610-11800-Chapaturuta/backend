namespace Frock_backend.Trips.Interfaces.REST.Resources;

public record TripHistoryResource(
    int Id,
    string RouteName,
    string OriginName,
    string DestinationName,
    string DriverName,
    string PassengerName,
    DateTime StartTime,
    DateTime? EndTime,
    decimal? Price,
    string Status,
    int FkIdRoute,
    int AvailableSeats,
    int? MyReservationId = null,
    string? MyReservationStatus = null,
    int? MyReservationSeats = null);
