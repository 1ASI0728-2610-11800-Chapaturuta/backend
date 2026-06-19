namespace Frock_backend.Trips.Application.Internal.CommandServices;

/// <summary>
///     Policy for how long a pending (unpaid) reservation keeps its seats reserved.
///     Bound from the "Reservations" configuration section.
/// </summary>
public class ReservationHoldOptions
{
    public const string SectionName = "Reservations";

    /// <summary>
    ///     Minutes a traveller has to complete payment before the seat hold is released. Default 15.
    /// </summary>
    public int PaymentHoldMinutes { get; set; } = 15;
}
