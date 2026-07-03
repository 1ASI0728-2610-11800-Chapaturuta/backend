using System.Text.Json;
using Frock_backend.Discovery.Domain.Model.ValueObjects;
using Frock_backend.Discovery.Domain.Services;

namespace Frock_backend.Discovery.Infrastructure.ExternalServices;

/// <summary>
///     Cliente del asistente sobre OpenRouter (API compatible con OpenAI, POST /chat/completions),
///     usando por defecto el modelo remoto configurado (p. ej. deepseek/deepseek-v4-flash).
///     Si OpenRouter no responde, cae a heurísticas deterministas (regex / plantilla / rechazo)
///     para que la función siga operativa y contenida en su dominio.
/// </summary>
public class OpenRouterChatAssistant(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OpenRouterChatAssistant> logger) : IChatAssistant
{
    private readonly string _model = configuration["Assistant:Model"] ?? "deepseek/deepseek-v4-flash";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<(string? Origin, string? Destination)> ExtractOriginDestinationAsync(string message)
    {
        // 1) Heurística determinista primero (rápida y robusta).
        var (o, d) = AssistantHeuristics.RegexExtract(message);
        if (!string.IsNullOrWhiteSpace(o) && !string.IsNullOrWhiteSpace(d)) return (o, d);

        // 2) LLM como respaldo para frases menos estructuradas.
        try
        {
            var content = await ChatAsync(AssistantHeuristics.ExtractSystem, message, timeoutSeconds: 20);
            if (content != null)
            {
                var json = AssistantHeuristics.ExtractJson(content);
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
            logger.LogWarning(ex, "OpenRouter extract falló; uso heurística");
        }
        return (o, d);
    }

    public async Task<string> NarrateAsync(string message, JourneyPlanResult plan)
    {
        var template = AssistantHeuristics.TemplateNarration(plan);
        try
        {
            var user = $"Mensaje del usuario: {message}\n\nItinerario calculado:\n{template}";
            var content = await ChatAsync(AssistantHeuristics.NarrateSystem, user, timeoutSeconds: 30);
            return string.IsNullOrWhiteSpace(content) ? template : content.Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenRouter narrate falló; uso plantilla");
            return template;
        }
    }

    public async Task<string> AnswerGroundedAsync(string message, string context)
    {
        try
        {
            var user = $"INFORMACIÓN DE CONTEXTO:\n{context}\n\nPregunta del usuario: {message}";
            var content = await ChatAsync(AssistantHeuristics.GroundedSystem, user, timeoutSeconds: 30);
            return string.IsNullOrWhiteSpace(content) ? AssistantHeuristics.GroundedFallback : content.Trim();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OpenRouter grounded answer falló; uso rechazo determinista");
            return AssistantHeuristics.GroundedFallback;
        }
    }

    // ── OpenRouter HTTP (compatible con OpenAI /chat/completions) ─────────────────
    private async Task<string?> ChatAsync(string system, string user, int timeoutSeconds)
    {
        var client = httpClientFactory.CreateClient("openrouter");
        var body = new
        {
            model = _model,
            temperature = 0.2,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user }
            }
        };
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var response = await client.PostAsJsonAsync("/api/v1/chat/completions", body, cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("OpenRouter devolvió {Status}", (int)response.StatusCode);
            return null;
        }
        var json = await response.Content.ReadAsStringAsync(cts.Token);
        var parsed = JsonSerializer.Deserialize<OpenRouterResponse>(json, JsonOpts);
        return parsed?.Choices?.FirstOrDefault()?.Message?.Content;
    }

    private sealed class ExtractDto
    {
        public string? Origin { get; set; }
        public string? Destination { get; set; }
    }
    private sealed class OpenRouterResponse
    {
        public List<OpenRouterChoice>? Choices { get; set; }
    }
    private sealed class OpenRouterChoice
    {
        public OpenRouterMessage? Message { get; set; }
    }
    private sealed class OpenRouterMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}
