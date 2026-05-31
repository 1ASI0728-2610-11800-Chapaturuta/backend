using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Interfaces.REST.Resources;

namespace Frock_backend.Payments.Interfaces.REST.Transform;

public static class RefundResourceFromEntityAssembler
{
    public static RefundResource ToResourceFromEntity(Refund entity) =>
        new RefundResource(
            entity.Id,
            entity.FkIdPayment,
            entity.Amount.Amount,
            entity.Amount.Currency,
            entity.Reason,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.ConfirmedAt
        );
}
