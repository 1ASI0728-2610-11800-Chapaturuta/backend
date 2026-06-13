namespace Frock_backend.Discovery.Interfaces.REST.Resources;

/// <summary>Petición al asistente Premium: el usuario y su consulta en lenguaje natural.</summary>
public record AssistantRequestResource(int UserId, string Message);
