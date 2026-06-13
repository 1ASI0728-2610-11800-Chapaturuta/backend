using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Model.ValueObjects;

namespace Frock_backend.Discovery.Domain.Services;

/// <summary>Respuesta del asistente: texto conversacional + itinerarios calculados.</summary>
public record AssistantReply(string Reply, List<JourneyItinerary> Itineraries);

public interface IAssistantQueryService
{
    Task<AssistantReply> Handle(PlanJourneyQuery query);
}
