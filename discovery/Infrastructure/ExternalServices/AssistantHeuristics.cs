using System.Text.RegularExpressions;
using Frock_backend.Discovery.Domain.Model.ValueObjects;

namespace Frock_backend.Discovery.Infrastructure.ExternalServices;

/// <summary>
///     Heurísticas deterministas compartidas por las implementaciones de <see cref="Domain.Services.IChatAssistant"/>.
///     Permiten que el asistente degrade con elegancia (sin LLM) y evitan duplicar lógica entre proveedores.
/// </summary>
internal static class AssistantHeuristics
{
    /// <summary>System prompt para extraer origen/destino en JSON.</summary>
    public const string ExtractSystem =
        "Extrae el origen y el destino de un viaje desde el mensaje del usuario. " +
        "Responde SOLO JSON: {\"origin\":\"...\",\"destination\":\"...\"}. " +
        "Si falta alguno, usa cadena vacía. No agregues texto extra.";

    /// <summary>System prompt para narrar un itinerario ya calculado (no inventar rutas).</summary>
    public const string NarrateSystem =
        "Eres un asistente de transporte urbano. Te doy un itinerario YA CALCULADO (no inventes rutas ni paraderos). " +
        "Explícalo en español, claro y breve (máx. 4 frases), mencionando dónde subir/bajar y los transbordos a pie. " +
        "Si no hay itinerario, dilo amablemente y sugiere reformular el origen/destino.";

    /// <summary>
    ///     System prompt con doble contrato: grounding (responder solo con el contexto) y
    ///     contención de dominio (rechazar preguntas fuera del transporte urbano de la app).
    /// </summary>
    public const string GroundedSystem =
        "Eres el asistente oficial de rutas de transporte urbano de esta app. " +
        "Respondes ÚNICAMENTE sobre rutas, paraderos, horarios, precios, cobertura y cómo moverse usando la red de la app. " +
        "Usa EXCLUSIVAMENTE la INFORMACIÓN DE CONTEXTO que se te entrega: no inventes rutas, paraderos, precios ni horarios. " +
        "Si la pregunta NO es sobre el transporte urbano de la app, o si el contexto no contiene la respuesta, " +
        "dilo con amabilidad y aclara que solo puedes ayudar con rutas y transporte de la app. " +
        "Responde en español, claro y breve (máx. 4 frases).";

    /// <summary>Rechazo determinista cuando el LLM no está disponible (mantiene la contención de dominio).</summary>
    public const string GroundedFallback =
        "Ahora mismo no puedo procesar tu consulta. Puedo ayudarte con rutas, paraderos, horarios y precios del transporte de la app; " +
        "vuelve a intentarlo en un momento.";

    /// <summary>Extrae origen/destino de frases estructuradas ("de X a Y", "desde X hasta Y"). (null, null) si no puede.</summary>
    public static (string? Origin, string? Destination) RegexExtract(string message)
    {
        var m = message.Trim();
        var patterns = new[]
        {
            @"desde\s+(?<o>.+?)\s+(?:hasta|a|hacia)\s+(?<d>.+)$",
            @"\bde\s+(?<o>.+?)\s+(?:hasta|a|hacia)\s+(?<d>.+)$",
            @"(?<o>.+?)\s+(?:hasta|hacia)\s+(?<d>.+)$",
        };
        foreach (var p in patterns)
        {
            var match = Regex.Match(m, p, RegexOptions.IgnoreCase);
            if (match.Success)
                return (Clean(match.Groups["o"].Value), Clean(match.Groups["d"].Value));
        }
        return (null, null);
    }

    /// <summary>Narración determinista de un itinerario (fallback cuando el LLM no está disponible).</summary>
    public static string TemplateNarration(JourneyPlanResult plan)
    {
        if (plan.Itineraries.Count == 0)
            return plan.Reason ?? "No encontré un itinerario para ese viaje.";

        var it = plan.Itineraries[0];
        var legs = it.Segments.Count(s => s.Kind == "ride");
        var parts = new List<string>();
        foreach (var seg in it.Segments)
        {
            if (seg.Kind == "ride")
                parts.Add($"toma una ruta desde \"{seg.From.Name}\" hasta \"{seg.To.Name}\"");
            else
                parts.Add($"camina ~{Math.Round(seg.Meters ?? 0)} m hasta \"{seg.To.Name}\"");
        }
        var transfers = it.Transfers == 0 ? "sin transbordos" : $"con {it.Transfers} transbordo(s)";
        return $"Itinerario ({legs} tramo(s), {transfers}): " + string.Join("; ", parts) +
               $". Precio estimado: S/ {it.TotalPrice:0.00}.";
    }

    /// <summary>Extrae el primer bloque JSON ({...}) de una respuesta del LLM. null si no hay.</summary>
    public static string? ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        return start >= 0 && end > start ? content.Substring(start, end - start + 1) : null;
    }

    private static string Clean(string s) =>
        Regex.Replace(s, @"[?¿!¡.]+$", "").Trim();
}
