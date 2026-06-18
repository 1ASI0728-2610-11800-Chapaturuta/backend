namespace Frock_backend.Trips.Domain.Model.Commands;

public record AssignDriverToTripCommand(int TripId, int DriverId);
