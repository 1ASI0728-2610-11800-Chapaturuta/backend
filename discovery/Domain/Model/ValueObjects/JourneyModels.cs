namespace Frock_backend.Discovery.Domain.Model.ValueObjects;

/// <summary>Referencia ligera a un paradero para los itinerarios del asistente.</summary>
public record StopRef(int Id, string Name, string Address, double? Latitude, double? Longitude);

/// <summary>
///     Un segmento de un itinerario: o un tramo en una ruta ("ride") o una caminata
///     de transbordo ("walk"). Se renderizan en orden en el frontend.
/// </summary>
public record JourneySegment(
    string Kind,            // "ride" | "walk"
    StopRef From,
    StopRef To,
    int? RouteId,           // solo en "ride"
    double? Price,          // precio de la ruta (solo "ride")
    double? Meters,         // distancia a pie (solo "walk")
    double? EtaSeconds      // ETA del tramo (OSRM); puede ser null
);

/// <summary>Un itinerario completo origen → destino, posiblemente con varios tramos y caminatas.</summary>
public record JourneyItinerary(
    List<JourneySegment> Segments,
    int Transfers,          // número de transbordos (legs - 1)
    double TotalPrice,
    double? TotalEtaSeconds
);

/// <summary>Resultado del planificador: el texto resuelto + los itinerarios encontrados.</summary>
public record JourneyPlanResult(
    string? ResolvedOrigin,
    string? ResolvedDestination,
    List<JourneyItinerary> Itineraries,
    string? Reason          // motivo cuando no se encontró itinerario (para narrar)
);
