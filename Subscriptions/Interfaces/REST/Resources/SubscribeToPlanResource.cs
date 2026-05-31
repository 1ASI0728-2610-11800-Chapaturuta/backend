using Frock_backend.Payments.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record SubscribeToPlanResource(
    [property: SwaggerSchema("ID del usuario que se suscribe al plan")]
    int FkIdUser,
    [property: SwaggerSchema("ID del plan al que se desea suscribir")]
    int FkIdPlan,
    [property: SwaggerSchema("Indica si la suscripcion se renovara automaticamente al finalizar el ciclo")]
    bool AutoRenew,
    [property: SwaggerSchema("Metodo de pago a utilizar para planes Premium: Yape, Plin, Card o Cash. Se ignora en planes Free")]
    PaymentMethod PaymentMethod
);
