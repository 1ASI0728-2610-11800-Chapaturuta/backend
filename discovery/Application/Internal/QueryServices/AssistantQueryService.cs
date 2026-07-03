using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Model.ValueObjects;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.routes.Domain.Model.ValueObjects;
using Frock_backend.routes.Domain.Service;
using Frock_backend.Subscriptions.Interfaces.ACL;

namespace Frock_backend.Discovery.Application.Internal.QueryServices;

/// <summary>
///     Orquesta el asistente Premium: valida la suscripción, extrae origen/destino,
///     pide el itinerario al planificador (grafo = fuente de verdad), lo enriquece con
///     ETA real (OSRM) y deja que el LLM lo narre.
/// </summary>
public class AssistantQueryService(
    ISubscriptionsContextFacade subscriptionsContextFacade,
    IJourneyPlanner journeyPlanner,
    IChatAssistant chatAssistant,
    IRouteKnowledgeRetriever routeKnowledgeRetriever,
    IOsrmRoutingService osrmRoutingService,
    IConfiguration configuration) : IAssistantQueryService
{
    private const double WalkSpeedMetersPerSecond = 1.4; // ~5 km/h

    // Feature flag: el Asistente IA aún está en evaluación (modelo/enfoque por definir).
    // Mientras "Assistant:Enabled" sea false, el endpoint responde "Próximamente" sin
    // ejecutar el planificador ni el LLM. Toda la lógica queda lista para producción.
    private readonly bool _enabled =
        bool.TryParse(configuration["Assistant:Enabled"], out var e) && e;

    public async Task<AssistantReply> Handle(PlanJourneyQuery query)
    {
        if (!_enabled)
            return new AssistantReply(
                "El Asistente IA estará disponible próximamente. Estamos afinando el modelo para darte las mejores rutas.",
                []);

        var isPremium = await subscriptionsContextFacade.HasActivePremiumPlanAsync(query.UserId);
        if (!isPremium)
            throw new UnauthorizedAccessException(
                "El Asistente IA de viajes es exclusivo del plan Premium. Suscríbete para usarlo.");

        if (string.IsNullOrWhiteSpace(query.Message))
            return new AssistantReply("Cuéntame a dónde quieres ir, por ejemplo: \"¿cómo llego de Surco a Comas?\"", []);

        var (origin, destination) = await chatAssistant.ExtractOriginDestinationAsync(query.Message);

        // Sin un par origen/destino claro no es un pedido de viaje: lo tratamos como pregunta
        // general del dominio. Retrieval estructurado + respuesta grounded (el system prompt
        // rechaza lo que caiga fuera del transporte de la app). No devuelve itinerarios.
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
        {
            var context = await routeKnowledgeRetriever.RetrieveAsync(query.Message);
            var groundedReply = await chatAssistant.AnswerGroundedAsync(query.Message, context);
            return new AssistantReply(groundedReply, []);
        }

        // Pedido de viaje: el grafo arma el itinerario (fuente de verdad); el LLM solo lo narra.
        var plan = await journeyPlanner.PlanAsync(origin, destination);
        plan = await EnrichWithEtaAsync(plan);

        var reply = await chatAssistant.NarrateAsync(query.Message, plan);
        return new AssistantReply(reply, plan.Itineraries);
    }

    // Calcula ETA real por tramo con OSRM y el total del itinerario.
    private async Task<JourneyPlanResult> EnrichWithEtaAsync(JourneyPlanResult plan)
    {
        if (plan.Itineraries.Count == 0) return plan;

        var newItineraries = new List<JourneyItinerary>();
        foreach (var it in plan.Itineraries)
        {
            var segments = new List<JourneySegment>();
            double? total = 0;
            foreach (var seg in it.Segments)
            {
                double? eta = seg.EtaSeconds;
                if (seg.Kind == "ride" &&
                    seg.From is { Latitude: not null, Longitude: not null } &&
                    seg.To is { Latitude: not null, Longitude: not null })
                {
                    try
                    {
                        var r = await osrmRoutingService.RouteAsync(new[]
                        {
                            new Coordinate(seg.From.Latitude!.Value, seg.From.Longitude!.Value),
                            new Coordinate(seg.To.Latitude!.Value, seg.To.Longitude!.Value)
                        });
                        eta = r.DurationSeconds;
                    }
                    catch { /* OSRM caído: dejamos el tramo sin ETA */ }
                }
                else if (seg.Kind == "walk" && seg.Meters.HasValue)
                {
                    eta = seg.Meters.Value / WalkSpeedMetersPerSecond;
                }

                if (total != null && eta != null) total += eta.Value;
                else if (eta == null && seg.Kind == "ride") total = null; // ETA incompleto

                segments.Add(seg with { EtaSeconds = eta });
            }
            newItineraries.Add(it with { Segments = segments, TotalEtaSeconds = total });
        }

        return plan with { Itineraries = newItineraries };
    }
}
