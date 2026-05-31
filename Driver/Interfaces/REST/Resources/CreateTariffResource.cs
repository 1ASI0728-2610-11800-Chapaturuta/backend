using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record CreateTariffResource(
    [property: SwaggerSchema("ID del conductor al que pertenece la tarifa")]
    int FkIdDriver,
    [property: SwaggerSchema("Tarifa base del servicio")]
    decimal BaseFare,
    [property: SwaggerSchema("Precio por kilometro")]
    decimal PricePerKm,
    [property: SwaggerSchema("Precio por minuto")]
    decimal PricePerMinute,
    [property: SwaggerSchema("Tarifa minima garantizada")]
    decimal MinFare,
    [property: SwaggerSchema("Moneda en formato ISO (por defecto PEN)")]
    string Currency,
    [property: SwaggerSchema("Dias de la semana disponibles para esta tarifa")]
    IEnumerable<DayOfWeek> AvailableDays
);
