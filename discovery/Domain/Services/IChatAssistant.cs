using Frock_backend.Discovery.Domain.Model.ValueObjects;

namespace Frock_backend.Discovery.Domain.Services;

/// <summary>
///     Capa de lenguaje natural del asistente. Hoy implementada con un LLM local (Ollama),
///     diseñada para cambiarse a Anthropic/Claude (modelo híbrido) sin tocar el planificador.
///     Toda implementación debe degradar con elegancia si el LLM no está disponible.
/// </summary>
public interface IChatAssistant
{
    /// <summary>Extrae origen y destino de un mensaje en lenguaje natural. (null, null) si no puede.</summary>
    Task<(string? Origin, string? Destination)> ExtractOriginDestinationAsync(string message);

    /// <summary>Redacta una respuesta conversacional describiendo el itinerario ya calculado.</summary>
    Task<string> NarrateAsync(string message, JourneyPlanResult plan);
}
