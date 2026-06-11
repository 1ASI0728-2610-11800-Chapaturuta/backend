namespace Frock_backend.Trips.Interfaces.ACL;

public interface ITripsContextFacade
{
    /// <summary>
    ///     Confirms a reservation once its associated payment has been completed.
    /// </summary>
    /// <param name="reservationId">The ID of the reservation to confirm.</param>
    Task ConfirmReservationAsync(int reservationId);
}
