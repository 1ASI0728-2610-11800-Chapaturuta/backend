using Frock_backend.Payments.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record CreatePaymentResource(
    [property: SwaggerSchema("ID del usuario que realiza el pago")]
    int FkIdUser,
    [property: SwaggerSchema("Monto a pagar (en la moneda indicada)")]
    decimal Amount,
    [property: SwaggerSchema("Moneda ISO (por defecto PEN)")]
    string Currency,
    [property: SwaggerSchema("Metodo de pago: Yape, Plin, Card o Cash")]
    PaymentMethod Method,
    [property: SwaggerSchema("Tipo de referencia que origina el pago (Reservation | Subscription)")]
    string ReferenceType,
    [property: SwaggerSchema("Identificador de la entidad referenciada (por ejemplo, ID de reserva)")]
    int ReferenceId
);
