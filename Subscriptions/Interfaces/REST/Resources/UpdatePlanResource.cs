using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record UpdatePlanResource(
    [property: SwaggerSchema("Nuevo precio del plan")]
    decimal Price,
    [property: SwaggerSchema("Descripcion actualizada de los beneficios del plan")]
    string Benefits,
    [property: SwaggerSchema("Nueva cuota mensual de consultas Discovery. Nulo para uso ilimitado")]
    int? DiscoveryQuota,
    [property: SwaggerSchema("Indica si el plan queda activo (true) o inactivo (false)")]
    bool IsActive
);
