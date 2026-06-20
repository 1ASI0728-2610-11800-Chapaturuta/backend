namespace Frock_backend.Trips.Domain.Model.Queries;

// Pending trips with no driver assigned yet on routes the requesting driver owns — the pool that
// driver can claim. UserId is the authenticated user; the driver identity is resolved from it.
public record GetAvailableTripsQuery(int UserId);
