namespace Frock_backend.Discovery.Domain.Model.Queries;

/// <summary>Consulta del asistente: el usuario (para el gate Premium) y su mensaje libre.</summary>
public record PlanJourneyQuery(int UserId, string Message);
