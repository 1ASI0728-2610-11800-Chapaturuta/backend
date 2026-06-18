using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Subscriptions.Interfaces.ACL;
using Frock_backend.Trips.Interfaces.ACL;

namespace Frock_backend.Payments.Application.Internal.CommandServices;

/// <summary>
///     Orchestrates payment confirmation and the activation of whatever the payment backs
///     (a reservation or a subscription).
///
///     Lives outside <see cref="PaymentCommandService"/> on purpose: wiring the cross-context
///     activation here avoids a circular dependency (Subscriptions -> Payments -> Subscriptions).
///     No service injects this orchestrator, so the dependency graph stays acyclic.
/// </summary>
public class PaymentConfirmationService(
    IPaymentCommandService paymentCommandService,
    IReservationNotificationService reservationNotificationService,
    ITripsContextFacade tripsContextFacade,
    ISubscriptionsContextFacade subscriptionsContextFacade)
{
    private const string ReservationReferenceType = "Reservation";
    private const string SubscriptionReferenceType = "Subscription";

    /// <summary>
    ///     Confirms the payment and, depending on what it references, confirms the reservation
    ///     or activates the subscription it backs.
    /// </summary>
    public async Task<Payment?> ConfirmAsync(int paymentId, string externalReference)
    {
        var payment = await paymentCommandService.Handle(new ConfirmPaymentCommand(paymentId, externalReference));
        if (payment == null) return null;

        if (string.Equals(payment.ReferenceType, ReservationReferenceType, StringComparison.OrdinalIgnoreCase))
        {
            await tripsContextFacade.ConfirmReservationAsync(payment.ReferenceId);
            await reservationNotificationService.NotifyReservationConfirmedAsync(payment);
        }
        else if (string.Equals(payment.ReferenceType, SubscriptionReferenceType, StringComparison.OrdinalIgnoreCase))
            await subscriptionsContextFacade.ActivateSubscriptionAsync(payment.ReferenceId);

        return payment;
    }
}
