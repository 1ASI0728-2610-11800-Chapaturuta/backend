using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record ConfirmPaymentResource(
    [property: SwaggerSchema("Referencia externa emitida por la pasarela (operacion, voucher, etc.)")]
    string ExternalReference
);
