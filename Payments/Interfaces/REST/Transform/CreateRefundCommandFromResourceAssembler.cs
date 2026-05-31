using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Interfaces.REST.Resources;

namespace Frock_backend.Payments.Interfaces.REST.Transform;

public static class CreateRefundCommandFromResourceAssembler
{
    public static CreateRefundCommand ToCommandFromResource(int paymentId, CreateRefundResource resource) =>
        new CreateRefundCommand(paymentId, resource.Amount, resource.Reason);
}
