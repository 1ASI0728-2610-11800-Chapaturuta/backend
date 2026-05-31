using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record UpdateTariffResource(
    [property: SwaggerSchema("Nueva tarifa base")]
    decimal BaseFare,
    [property: SwaggerSchema("Nuevo precio por kilometro")]
    decimal PricePerKm,
    [property: SwaggerSchema("Nuevo precio por minuto")]
    decimal PricePerMinute,
    [property: SwaggerSchema("Nueva tarifa minima")]
    decimal MinFare,
    [property: SwaggerSchema("Nuevos dias de la semana disponibles")]
    IEnumerable<DayOfWeek> AvailableDays
);
