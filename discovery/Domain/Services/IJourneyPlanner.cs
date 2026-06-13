using Frock_backend.Discovery.Domain.Model.ValueObjects;

namespace Frock_backend.Discovery.Domain.Services;

/// <summary>
///     Planificador determinista de viajes multi-tramo sobre la red de rutas/paraderos.
///     Encuentra conexiones reales (con transbordos a pie entre paraderos cercanos);
///     es la fuente de verdad: la IA solo narra lo que este planificador calcula.
/// </summary>
public interface IJourneyPlanner
{
    Task<JourneyPlanResult> PlanAsync(string? originText, string? destinationText);
}
