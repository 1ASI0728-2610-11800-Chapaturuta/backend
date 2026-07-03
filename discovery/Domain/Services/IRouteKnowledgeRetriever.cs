namespace Frock_backend.Discovery.Domain.Services;

/// <summary>
///     Retrieval estructurado (no vectorial) sobre la red de transporte: dada una consulta en
///     lenguaje natural, arma un snapshot de texto compacto con las rutas/paraderos más relevantes
///     para servir de contexto (grounding) al asistente. La red en MySQL es la única fuente de verdad.
/// </summary>
public interface IRouteKnowledgeRetriever
{
    /// <summary>
    ///     Devuelve texto estructurado con las rutas relevantes a la consulta, o cadena vacía si
    ///     la red no tiene datos. El asistente debe rechazar la pregunta si el contexto no la cubre.
    /// </summary>
    Task<string> RetrieveAsync(string query);
}
