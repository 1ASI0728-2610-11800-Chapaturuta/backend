namespace Frock_backend.Trips.Domain.Model.Queries;

// Read model for trip history with resolved names (F6).
public record TripHistoryView(
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
    // Reservation of the querying passenger on this trip (null when none / not a reservation-derived row).
    int? MyReservationId = null,
    string? MyReservationStatus = null,
    int? MyReservationSeats = null);
