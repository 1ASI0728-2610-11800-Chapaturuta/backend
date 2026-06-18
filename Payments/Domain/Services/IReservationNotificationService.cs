using Frock_backend.Payments.Domain.Model.Aggregates;

namespace Frock_backend.Payments.Domain.Services;

public interface IReservationNotificationService
{
    Task NotifyReservationConfirmedAsync(Payment payment);
}
