using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST.Resources;

public record PayUChargeResource(
    [property: SwaggerSchema("Token de tarjeta generado por PayU JS en el frontend")]
    string CardToken,
    [property: SwaggerSchema("Nombre completo del titular de la tarjeta")]
    string PayerFullName,
    [property: SwaggerSchema("Email del pagador (recibirá el comprobante)")]
    string PayerEmail,
    [property: SwaggerSchema("DNI del pagador")]
    string PayerDocumentNumber,
    [property: SwaggerSchema("Marca de la tarjeta: VISA, MASTERCARD, AMEX, DINERS")]
    string PaymentMethodBrand,
    [property: SwaggerSchema("IP del navegador del pagador (antifraude)")]
    string PayerIpAddress,
    [property: SwaggerSchema("ID de sesión del dispositivo generado por PayU JS (antifraude)")]
    string DeviceSessionId,
    [property: SwaggerSchema("User-Agent del navegador del pagador")]
    string PayerUserAgent,
    [property: SwaggerSchema("Cookie del navegador del pagador")]
    string PayerCookie
);
