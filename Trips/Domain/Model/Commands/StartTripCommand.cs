namespace Frock_backend.Trips.Domain.Model.Commands;

// RequestingUserId is the authenticated user starting the trip; the driver identity and route
// ownership are validated from it so a driver cannot start trips on routes they don't own.
public record StartTripCommand(int TripId, int RequestingUserId);
