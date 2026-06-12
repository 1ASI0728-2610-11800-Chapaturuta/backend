using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record PayUChargeResource(
    [property: SwaggerSchema("Número de la tarjeta (solo dígitos)")]
    string CardNumber,
    [property: SwaggerSchema("Código de seguridad (CVV)")]
    string CardSecurityCode,
    [property: SwaggerSchema("Fecha de vencimiento en formato YYYY/MM")]
    string CardExpirationDate,
    [property: SwaggerSchema("Nombre del titular tal como aparece en la tarjeta")]
    string CardHolderName,
    [property: SwaggerSchema("Nombre completo del pagador")]
    string PayerFullName,
    [property: SwaggerSchema("Email del pagador (recibirá el comprobante)")]
    string PayerEmail,
    [property: SwaggerSchema("DNI del pagador")]
    string PayerDocumentNumber,
    [property: SwaggerSchema("Marca de la tarjeta: VISA, MASTERCARD, AMEX, DINERS")]
    string PaymentMethodBrand,
    [property: SwaggerSchema("ID de sesión del dispositivo (antifraude)")]
    string DeviceSessionId,
    [property: SwaggerSchema("IP del pagador (antifraude). Si se omite, se toma de la petición")]
    string? PayerIpAddress = null,
    [property: SwaggerSchema("User-Agent del pagador. Si se omite, se toma de la petición")]
    string? PayerUserAgent = null,
    [property: SwaggerSchema("Cookie del navegador del pagador")]
    string? PayerCookie = null
);
