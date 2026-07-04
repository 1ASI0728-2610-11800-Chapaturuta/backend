using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Model.Queries;
using Frock_backend.Payments.Interfaces.REST.Resources;

namespace Frock_backend.Payments.Interfaces.REST.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(Payment entity, string? payerName = null) =>
        new PaymentResource(
            entity.Id,
            entity.FkIdUser,
            entity.Amount.Amount,
            entity.Amount.Currency,
            entity.Method.ToString(),
            entity.Status.ToString(),
            entity.ExternalReference,
            entity.ReferenceType,
            entity.ReferenceId,
            entity.CreatedAt,
            entity.ConfirmedAt,
            payerName
        );

    public static PaymentResource ToResourceFromView(ReceivedPaymentView view) =>
        ToResourceFromEntity(view.Payment, view.PayerName);
}
