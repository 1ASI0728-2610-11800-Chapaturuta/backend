using System.Text.Json;
using System.Text.RegularExpressions;
using Frock_backend.Discovery.Domain.Model.ValueObjects;
using Frock_backend.Discovery.Domain.Services;

namespace Frock_backend.Discovery.Infrastructure.ExternalServices;

/// <summary>
///     Cliente del asistente sobre un LLM local de Ollama (POST /api/chat).
///     Si Ollama no responde, cae a heurísticas deterministas (regex / plantilla)
///     para que la función siga operativa en el demo.
/// </summary>
public class OllamaChatAssistant(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OllamaChatAssistant> logger) : IChatAssistant
{
    private readonly string _model = configuration["Assistant:Model"] ?? "llama3.1";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<(string? Origin, string? Destination)> ExtractOriginDestinationAsync(string message)
    {
        // 1) Heurística determinista primero (rápida y robusta).
        var (o, d) = RegexExtract(message);
        if (!string.IsNullOrWhiteSpace(o) && !string.IsNullOrWhiteSpace(d)) return (o, d);

        // 2) LLM como respaldo para frases menos estructuradas.
        try
        {
            var system = "Extrae el origen y el destino de un viaje desde el mensaje del usuario. " +
                         "Responde SOLO JSON: {\"origin\":\"...\",\"destination\":\"...\"}. " +
                         "Si falta alguno, usa cadena vacía. No agregues texto extra.";
            var content = await ChatAsync(system, message, timeoutSeconds: 20);
            if (content != null)
            {
                var json = ExtractJson(content);
                if (json != null)
                {
                    var parsed = JsonSerializer.Deserialize<ExtractDto>(json, JsonOpts);
                    var oo = string.IsNullOrWhiteSpace(parsed?.Origin) ? o : parsed!.Origin;
                    var dd = string.IsNullOrWhiteSpace(parsed?.Destination) ? d : parsed!.Destination;
                    return (oo, dd);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama extract falló; uso heurística");
        }
        return (o, d);
    }

    public async Task<string> NarrateAsync(string message, JourneyPlanResult plan)
    {
        var template = TemplateNarration(plan);
        try
        {
            var system =
                "Eres un asistente de transporte urbano. Te doy un itinerario YA CALCULADO (no inventes rutas ni paraderos). " +
                "Explícalo en español, claro y breve (máx. 4 frases), mencionando dónde subir/bajar y los transbordos a pie. " +
                "Si no hay itinerario, dilo amablemente y sugiere reformular el origen/destino.";
            var user = $"Mensaje del usuario: {message}\n\nItinerario calculado:\n{template}";
            var content = await ChatAsync(system, user, timeoutSeconds: 30);
            return string.IsNullOrWhiteSpace(content) ? template : content.Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Ollama narrate falló; uso plantilla");
            return template;
        }
    }

    // ── Ollama HTTP ──────────────────────────────────────────────────────────────
    private async Task<string?> ChatAsync(string system, string user, int timeoutSeconds)
    {
        var client = httpClientFactory.CreateClient("ollama");
        var body = new
        {
            model = _model,
            stream = false,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var response = await client.PostAsJsonAsync("/api/chat", body, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Ollama devolvió {Status}", (int)response.StatusCode);
            return null;
        }
        var json = await response.Content.ReadAsStringAsync(cts.Token);
        var parsed = JsonSerializer.Deserialize<OllamaChatResponse>(json, JsonOpts);
        return parsed?.Message?.Content;
    }

    // ── Heurísticas deterministas (fallback) ─────────────────────────────────────
    private static (string?, string?) RegexExtract(string message)
    {
        var m = message.Trim();
        // "de X a Y", "desde X hasta Y", "desde X a Y"
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

    private static string Clean(string s) =>
        Regex.Replace(s, @"[?¿!¡.]+$", "").Trim();

    private static string TemplateNarration(JourneyPlanResult plan)
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

    private static string? ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        return start >= 0 && end > start ? content.Substring(start, end - start + 1) : null;
    }

    private sealed class ExtractDto
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
    }
    private sealed class OllamaChatResponse
    {
        public OllamaMessage? Message { get; set; }
    }
    private sealed class OllamaMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}
