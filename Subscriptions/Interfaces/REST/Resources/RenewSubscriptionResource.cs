using Frock_backend.Payments.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record RenewSubscriptionResource(
    [property: SwaggerSchema("Metodo de pago a utilizar para la renovacion del plan Premium")]
    PaymentMethod PaymentMethod
);
