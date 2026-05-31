using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Driver.Interfaces.REST.Resources;

public record SetRouteDurationResource(
    [property: SwaggerSchema("ID de la ruta")]
    int FkIdRoute,
    [property: SwaggerSchema("Tiempo estimado de duracion del viaje en minutos")]
    int EstimatedMinutes
);
