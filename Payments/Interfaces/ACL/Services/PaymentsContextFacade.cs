using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Model.ValueObjects;
using Frock_backend.Payments.Domain.Services;

namespace Frock_backend.Payments.Interfaces.ACL.Services;

public class PaymentsContextFacade(
    IPaymentCommandService paymentCommandService,
    IRefundCommandService refundCommandService) : IPaymentsContextFacade
{
    /**
     * <summary>
     *     Registers a new payment in Pending status and triggers the gateway initiation.
     * </summary>
     * <param name="userId">The ID of the user paying.</param>
     * <param name="amount">The payment amount (positive decimal).</param>
     * <param name="method">The payment method (Yape, Plin, Card, Cash).</param>
     * <param name="referenceType">The type of the referenced entity (Reservation | Subscription).</param>
     * <param name="referenceId">The ID of the referenced entity.</param>
     * <returns>The ID of the newly created payment, or 0 if creation failed.</returns>
     */
    public async Task<int> RegisterPendingPaymentAsync(int userId, decimal amount, PaymentMethod method, string referenceType, int referenceId)
    {
        var command = new CreatePaymentCommand(userId, amount, "PEN", method, referenceType, referenceId);
        var payment = await paymentCommandService.Handle(command);
        return payment?.Id ?? 0;
    }

    /**
     * <summary>
     *     Confirms a previously registered payment using the gateway external reference.
     * </summary>
     * <param name="paymentId">The ID of the payment to confirm.</param>
     * <param name="externalReference">The external reference issued by the gateway.</param>
     */
    public async Task ConfirmPaymentAsync(int paymentId, string externalReference)
    {
        var command = new ConfirmPaymentCommand(paymentId, externalReference);
        await paymentCommandService.Handle(command);
    }

    /**
     * <summary>
     *     Registers a refund against an existing payment.
     * </summary>
     * <param name="paymentId">The ID of the payment being refunded.</param>
     * <param name="amount">The refund amount.</param>
     * <param name="reason">The reason for the refund.</param>
     * <returns>The ID of the newly created refund, or 0 if creation failed.</returns>
     */
    public async Task<int> RegisterRefundAsync(int paymentId, decimal amount, string reason)
    {
        var command = new CreateRefundCommand(paymentId, amount, reason);
        var refund = await refundCommandService.Handle(command);
        return refund?.Id ?? 0;
    }

    /**
     * <summary>
     *     Marks a pending payment as failed. Best-effort: swallows the invalid-transition error
     *     for payments that are no longer pending, so callers (e.g. expiring a reservation hold)
     *     never fail because the payment moved on independently.
     * </summary>
     * <param name="paymentId">The ID of the payment to fail.</param>
     */
    public async Task FailPaymentAsync(int paymentId)
    {
        try
        {
            await paymentCommandService.Handle(new FailPaymentCommand(paymentId));
        }
        catch (InvalidOperationException)
        {
            // Payment was not pending (already confirmed/failed). Nothing to do.
        }
    }
}
