using System.Text;
using System.Text.RegularExpressions;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Repository;

namespace Frock_backend.Discovery.Application.Internal.Services;

/// <summary>
///     Retrieval estructurado sobre las rutas activas: puntúa cada ruta por solapamiento de
///     términos de la consulta contra sus paraderos/direcciones y arma un contexto compacto con
///     las mejores coincidencias. Sin embeddings ni vector DB — la data ya es estructurada y el
///     grafo (JourneyPlannerService) sigue siendo la fuente de verdad para itinerarios.
/// </summary>
public class RouteKnowledgeRetriever(IRouteRepository routeRepository) : IRouteKnowledgeRetriever
{
    private const int MaxRoutes = 8;        // techo de rutas en el contexto (controla tamaño del prompt)
    private const int MaxStopsPerRoute = 10; // techo de paraderos listados por ruta
    private const int MinTermLength = 3;     // ignora términos muy cortos ("de", "a", "en"...)

    // Palabras vacías frecuentes en consultas de transporte (no aportan a la relevancia).
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "el", "los", "las", "en", "un", "una", "para", "por", "con", "al",
        "que", "cual", "como", "hasta", "hacia", "desde", "ruta", "rutas", "paradero", "paraderos",
        "quiero", "llegar", "hay", "cuesta", "precio", "horario", "pasa", "cerca", "donde",
        "mas", "barata", "cara", "cuanto", "cuantos", "hora", "sale", "primera", "ultima"
    };

    public async Task<string> RetrieveAsync(string query)
    {
        var routes = (await routeRepository.ListRoutes())
            .Where(r => r.IsActive)
            .ToList();
        if (routes.Count == 0) return string.Empty;

        var terms = Tokenize(query);

        // Puntúa por coincidencia de términos; si no hay términos útiles, cae a un panorama general.
        List<RouteAggregate> selected;
        if (terms.Count == 0)
        {
            selected = routes.Take(MaxRoutes).ToList();
        }
        else
        {
            var scored = routes
                .Select(r => (Route: r, Score: Score(r, terms)))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Route)
                .Take(MaxRoutes)
                .ToList();

            // Sin coincidencias: entregamos un panorama general para preguntas amplias del dominio.
            selected = scored.Count > 0 ? scored : routes.Take(MaxRoutes).ToList();
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Red de transporte de la app — {routes.Count} ruta(s) activa(s). Rutas relevantes:");
        foreach (var r in selected)
            sb.AppendLine(Describe(r));

        return sb.ToString().TrimEnd();
    }

    private static int Score(RouteAggregate route, List<string> terms)
    {
        var haystack = Haystack(route);
        return terms.Count(term => haystack.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static string Haystack(RouteAggregate route)
    {
        var sb = new StringBuilder();
        foreach (var rs in route.Stops)
        {
            if (rs.Stop is null) continue;
            sb.Append(rs.Stop.Name).Append(' ');
            if (!string.IsNullOrWhiteSpace(rs.Stop.Address)) sb.Append(rs.Stop.Address).Append(' ');
        }
        return sb.ToString();
    }

    private static string Describe(RouteAggregate route)
    {
        var stops = route.Stops
            .Where(rs => rs.Stop is not null)
            .Select(rs => rs.Stop.Name)
            .ToList();

        var endpoints = stops.Count switch
        {
            0 => "(sin paraderos)",
            1 => stops[0],
            _ => $"{stops[0]} → {stops[^1]}"
        };

        var listed = stops.Take(MaxStopsPerRoute);
        var stopsText = stops.Count > MaxStopsPerRoute
            ? string.Join(", ", listed) + $", … (+{stops.Count - MaxStopsPerRoute})"
            : string.Join(", ", listed);

        // "frecuencia de salida" = lenguaje ubicuo: cada cuánto llega un nuevo vehículo
        // al paradero para iniciar la ruta.
        var line = $"- Ruta #{route.Id}: {endpoints}. Precio S/ {route.Price:0.00}. " +
                   $"Duración ~{route.Duration} min, frecuencia de salida ~{route.Frequency} min.";
        if (!string.IsNullOrWhiteSpace(stopsText))
            line += $" Paraderos: {stopsText}.";
        return line;
    }

    private static List<string> Tokenize(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        return Regex.Split(query.ToLowerInvariant(), @"[^\p{L}\p{N}]+")
            .Where(t => t.Length >= MinTermLength && !StopWords.Contains(t))
            .Distinct()
            .ToList();
    }
}
