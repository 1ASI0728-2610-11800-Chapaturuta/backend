using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Subscriptions.Interfaces.REST.Resources;

public record CreatePlanResource(
    [property: SwaggerSchema("Nombre comercial del plan (por ejemplo, Free o Premium)")]
    string Name,
    [property: SwaggerSchema("Tipo de plan: Free o Premium")]
    PlanType PlanType,
    [property: SwaggerSchema("Rol objetivo al que aplica el plan: Traveller, Driver o Both")]
    TargetRole TargetRole,
    [property: SwaggerSchema("Precio del plan en la moneda indicada")]
    decimal Price,
    [property: SwaggerSchema("Moneda ISO en la que se cobra el plan (por defecto PEN)")]
    string Currency,
    [property: SwaggerSchema("Ciclo de facturacion del plan: Monthly o Yearly")]
    BillingCycle BillingCycle,
    [property: SwaggerSchema("Descripcion textual de los beneficios incluidos en el plan")]
    string Benefits,
    [property: SwaggerSchema("Cuota mensual de consultas Discovery con IA. Dejar nulo para uso ilimitado (Premium)")]
    int? DiscoveryQuota
);
