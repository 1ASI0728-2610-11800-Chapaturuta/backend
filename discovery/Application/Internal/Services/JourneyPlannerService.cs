using Frock_backend.Discovery.Domain.Model.ValueObjects;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.routes.Domain.Model.Aggregates;
using Frock_backend.routes.Domain.Repository;
using Frock_backend.stops.Domain.Model.Aggregates;

namespace Frock_backend.Discovery.Application.Internal.Services;

/// <summary>
///     Construye un grafo de la red (paraderos = nodos; aristas de viaje dentro de una ruta
///     y aristas de transbordo a pie entre paraderos cercanos) y busca, con Dijkstra por
///     (transbordos, caminatas), el mejor itinerario origen → destino — incluso cuando no
///     existe una ruta directa.
/// </summary>
public class JourneyPlannerService(
    IRouteRepository routeRepository,
    IConfiguration configuration) : IJourneyPlanner
{
    private readonly double _walkRadiusMeters =
        double.TryParse(configuration["Journey:WalkRadiusMeters"], out var r) ? r : 300d;
    private readonly int _maxLegs =
        int.TryParse(configuration["Journey:MaxLegs"], out var m) ? m : 3;

    public async Task<JourneyPlanResult> PlanAsync(string? originText, string? destinationText)
    {
        if (string.IsNullOrWhiteSpace(originText) || string.IsNullOrWhiteSpace(destinationText))
            return new JourneyPlanResult(originText, destinationText, [], "Necesito un origen y un destino.");

        var routes = (await routeRepository.ListRoutes())
            .Where(r => r.IsActive)
            .ToList();

        // Paraderos con coordenadas (necesarias para el grafo / caminatas).
        var stopsById = new Dictionary<int, Stop>();
        foreach (var route in routes)
            foreach (var rs in route.Stops)
                if (rs.Stop is { Latitude: not null, Longitude: not null } && !stopsById.ContainsKey(rs.Stop.Id))
                    stopsById[rs.Stop.Id] = rs.Stop;

        if (stopsById.Count == 0)
            return new JourneyPlanResult(originText, destinationText, [], "Aún no hay paraderos con ubicación en la red.");

        var originStops = MatchStops(stopsById.Values, originText);
        var destStops = MatchStops(stopsById.Values, destinationText);

        if (originStops.Count == 0)
            return new JourneyPlanResult(originText, destinationText, [], $"No encontré paraderos para el origen \"{originText}\".");
        if (destStops.Count == 0)
            return new JourneyPlanResult(originText, destinationText, [], $"No encontré paraderos para el destino \"{destinationText}\".");

        // Aristas de viaje: dentro de cada ruta, de un paradero a cualquier paradero posterior.
        // (rideEdges[stopId] = lista de (toStopId, routeId, price))
        var rideEdges = new Dictionary<int, List<(int To, int RouteId, double Price)>>();
        foreach (var route in routes)
        {
            var ordered = route.Stops
                .Where(rs => rs.Stop is { Latitude: not null, Longitude: not null })
                .Select(rs => rs.Stop.Id)
                .ToList();
            for (var i = 0; i < ordered.Count; i++)
                for (var j = i + 1; j < ordered.Count; j++)
                {
                    if (!rideEdges.TryGetValue(ordered[i], out var list))
                        rideEdges[ordered[i]] = list = [];
                    list.Add((ordered[j], route.Id, route.Price));
                }
        }

        // Aristas de transbordo a pie: pares de paraderos a <= radio (Haversine).
        var walkEdges = new Dictionary<int, List<(int To, double Meters)>>();
        var stopList = stopsById.Values.ToList();
        for (var i = 0; i < stopList.Count; i++)
            for (var j = i + 1; j < stopList.Count; j++)
            {
                var meters = HaversineMeters(
                    stopList[i].Latitude!.Value, stopList[i].Longitude!.Value,
                    stopList[j].Latitude!.Value, stopList[j].Longitude!.Value);
                if (meters <= _walkRadiusMeters)
                {
                    AddWalk(walkEdges, stopList[i].Id, stopList[j].Id, meters);
                    AddWalk(walkEdges, stopList[j].Id, stopList[i].Id, meters);
                }
            }

        var itinerary = FindBest(originStops, destStops.Select(s => s.Id).ToHashSet(), rideEdges, walkEdges, stopsById);
        if (itinerary == null)
            return new JourneyPlanResult(originText, destinationText, [],
                "No encontré una combinación de rutas (ni con transbordos a pie) para ese destino.");

        return new JourneyPlanResult(originText, destinationText, [itinerary], null);
    }

    // ── Dijkstra por (transbordos, caminatas) ────────────────────────────────────
    private JourneyItinerary? FindBest(
        List<Stop> originStops,
        HashSet<int> destStopIds,
        Dictionary<int, List<(int To, int RouteId, double Price)>> rideEdges,
        Dictionary<int, List<(int To, double Meters)>> walkEdges,
        Dictionary<int, Stop> stopsById)
    {
        // Costo lexicográfico: (legs, walks). prev guarda cómo se llegó a cada nodo.
        var dist = new Dictionary<int, (int Legs, int Walks)>();
        var prev = new Dictionary<int, (int From, string Kind, int RouteId, double Price, double Meters)>();
        var visited = new HashSet<int>();

        foreach (var s in originStops)
            dist[s.Id] = (0, 0); // todos los paraderos de origen arrancan sin tramos ni caminatas

        while (true)
        {
            // Extraer el nodo no visitado con menor costo.
            int? bestId = null;
            (int Legs, int Walks) best = (int.MaxValue, int.MaxValue);
            foreach (var kv in dist)
            {
                if (visited.Contains(kv.Key)) continue;
                if (bestId == null || Less(kv.Value, best)) { bestId = kv.Key; best = kv.Value; }
            }
            if (bestId == null) break;
            visited.Add(bestId.Value);

            if (destStopIds.Contains(bestId.Value))
                return Reconstruct(bestId.Value, prev, stopsById);

            // Relajar aristas de viaje (un tramo más).
            if (best.Legs < _maxLegs && rideEdges.TryGetValue(bestId.Value, out var rides))
            {
                foreach (var (to, routeId, price) in rides)
                {
                    var cand = (best.Legs + 1, best.Walks);
                    if (!dist.TryGetValue(to, out var d) || Less(cand, d))
                    {
                        dist[to] = cand;
                        prev[to] = (bestId.Value, "ride", routeId, price, 0);
                    }
                }
            }

            // Relajar aristas a pie (un transbordo caminando más).
            if (walkEdges.TryGetValue(bestId.Value, out var walks))
            {
                foreach (var (to, meters) in walks)
                {
                    var cand = (best.Legs, best.Walks + 1);
                    if (!dist.TryGetValue(to, out var d) || Less(cand, d))
                    {
                        dist[to] = cand;
                        prev[to] = (bestId.Value, "walk", 0, 0, meters);
                    }
                }
            }
        }

        return null;
    }

    private static JourneyItinerary Reconstruct(
        int destId,
        Dictionary<int, (int From, string Kind, int RouteId, double Price, double Meters)> prev,
        Dictionary<int, Stop> stopsById)
    {
        var segments = new List<JourneySegment>();
        var current = destId;
        while (prev.TryGetValue(current, out var p))
        {
            var from = Ref(stopsById[p.From]);
            var to = Ref(stopsById[current]);
            segments.Add(p.Kind == "ride"
                ? new JourneySegment("ride", from, to, p.RouteId, p.Price, null, null)
                : new JourneySegment("walk", from, to, null, null, p.Meters, null));
            current = p.From;
        }
        segments.Reverse();

        var legs = segments.Count(s => s.Kind == "ride");
        var totalPrice = segments.Where(s => s.Kind == "ride").Sum(s => s.Price ?? 0);
        return new JourneyItinerary(segments, Math.Max(0, legs - 1), totalPrice, null);
    }

    private static List<Stop> MatchStops(IEnumerable<Stop> stops, string text)
    {
        var t = text.Trim().ToLowerInvariant();
        return stops.Where(s =>
            (s.Name?.ToLowerInvariant().Contains(t) ?? false) ||
            (s.Address?.ToLowerInvariant().Contains(t) ?? false)).ToList();
    }

    private static void AddWalk(Dictionary<int, List<(int To, double Meters)>> map, int from, int to, double meters)
    {
        if (!map.TryGetValue(from, out var list)) map[from] = list = [];
        list.Add((to, meters));
    }

    private static StopRef Ref(Stop s) => new(s.Id, s.Name, s.Address, s.Latitude, s.Longitude);

    private static bool Less((int Legs, int Walks) a, (int Legs, int Walks) b) =>
        a.Legs != b.Legs ? a.Legs < b.Legs : a.Walks < b.Walks;

    private static double HaversineMeters(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371000; // metros
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
