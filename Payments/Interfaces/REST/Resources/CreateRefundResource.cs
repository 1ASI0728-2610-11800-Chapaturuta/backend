using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record CreateRefundResource(
    [property: SwaggerSchema("Monto a reembolsar (debe ser menor o igual al monto del pago)")]
    decimal Amount,
    [property: SwaggerSchema("Motivo del reembolso")]
    string Reason
);
