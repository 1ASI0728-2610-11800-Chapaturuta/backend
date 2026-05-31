using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Interfaces.REST.Resources;

namespace Frock_backend.Payments.Interfaces.REST.Transform;

public static class CreatePaymentCommandFromResourceAssembler
{
    public static CreatePaymentCommand ToCommandFromResource(CreatePaymentResource resource) =>
        new CreatePaymentCommand(
            resource.FkIdUser,
            resource.Amount,
            string.IsNullOrWhiteSpace(resource.Currency) ? "PEN" : resource.Currency,
            resource.Method,
            resource.ReferenceType,
            resource.ReferenceId
        );
}
