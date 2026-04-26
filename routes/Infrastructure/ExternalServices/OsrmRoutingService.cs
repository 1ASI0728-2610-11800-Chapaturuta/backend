using System.Text.Json;
using Frock_backend.routes.Domain.Exceptions;
using Frock_backend.routes.Domain.Model.ValueObjects;
using Frock_backend.routes.Domain.Service;

namespace Frock_backend.routes.Infrastructure.ExternalServices;

public class OsrmRoutingService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OsrmRoutingService> logger) : IOsrmRoutingService
{
    private readonly string _profile = configuration["Osrm:Profile"] ?? "driving";
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<OsrmRouteResult> RouteAsync(IEnumerable<Coordinate> waypoints)
    {
        var points = waypoints.ToList();
        if (points.Count < 2)
            throw new ArgumentException("At least 2 waypoints required.");

        var coords = string.Join(";", points.Select(p => $"{p.Longitude},{p.Latitude}"));
        var url = $"/route/v1/{_profile}/{coords}?overview=full&geometries=polyline&steps=false";

        return await WithRetry(async () =>
        {
            var client = httpClientFactory.CreateClient("osrm");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await client.GetAsync(url);
                sw.Stop();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("OSRM returned {Status} for RouteAsync", (int)response.StatusCode);
                    throw new OsrmUnavailableException($"OSRM returned {(int)response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OsrmRouteResponse>(json, JsonOpts)
                    ?? throw new OsrmUnavailableException("Empty response from OSRM");

                if (result.Code != "Ok" || result.Routes == null || result.Routes.Count == 0)
                    throw new OsrmUnavailableException($"OSRM code: {result.Code}");

                var route = result.Routes[0];
                logger.LogInformation("OSRM RouteAsync latency={Latency}ms distance={Distance}m duration={Duration}s",
                    sw.ElapsedMilliseconds, route.Distance, route.Duration);

                return new OsrmRouteResult(route.Distance, route.Duration, route.Geometry);
            }
            catch (TaskCanceledException ex)
            {
                sw.Stop();
                logger.LogError(ex, "OSRM RouteAsync timeout after {Latency}ms", sw.ElapsedMilliseconds);
                throw new OsrmUnavailableException("OSRM request timed out", ex);
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                logger.LogError(ex, "OSRM RouteAsync HTTP error after {Latency}ms", sw.ElapsedMilliseconds);
                throw new OsrmUnavailableException("OSRM unreachable", ex);
            }
        });
    }

    public async Task<IEnumerable<double>> TableAsync(Coordinate source, IEnumerable<Coordinate> destinations)
    {
        var dests = destinations.ToList();
        if (dests.Count == 0) return [];

        var allCoords = new[] { source }.Concat(dests);
        var coords = string.Join(";", allCoords.Select(p => $"{p.Longitude},{p.Latitude}"));
        var destIndices = string.Join(";", Enumerable.Range(1, dests.Count));
        var url = $"/table/v1/{_profile}/{coords}?sources=0&destinations={destIndices}";

        var client = httpClientFactory.CreateClient("osrm");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await client.GetAsync(url);
            sw.Stop();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OSRM TableAsync fallback: {Status}", (int)response.StatusCode);
                return Enumerable.Repeat(double.MaxValue, dests.Count);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OsrmTableResponse>(json, JsonOpts);
            logger.LogInformation("OSRM TableAsync latency={Latency}ms", sw.ElapsedMilliseconds);

            return result?.Durations?[0]?.Select(d => d ?? double.MaxValue)
                ?? Enumerable.Repeat(double.MaxValue, dests.Count);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "OSRM TableAsync failed after {Latency}ms, using fallback", sw.ElapsedMilliseconds);
            return Enumerable.Repeat(double.MaxValue, dests.Count);
        }
    }

    public async Task<Coordinate?> NearestAsync(Coordinate point)
    {
        var url = $"/nearest/v1/{_profile}/{point.Longitude},{point.Latitude}";
        var client = httpClientFactory.CreateClient("osrm");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await client.GetAsync(url);
            sw.Stop();
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OsrmNearestResponse>(json, JsonOpts);
            logger.LogInformation("OSRM NearestAsync latency={Latency}ms", sw.ElapsedMilliseconds);

            var loc = result?.Waypoints?.FirstOrDefault()?.Location;
            if (loc == null || loc.Count < 2) return null;
            return new Coordinate(loc[1], loc[0]);
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogWarning(ex, "OSRM NearestAsync failed after {Latency}ms", sw.ElapsedMilliseconds);
            return null;
        }
    }

    private static async Task<T> WithRetry<T>(Func<Task<T>> operation, int maxRetries = 2)
    {
        var delay = TimeSpan.FromMilliseconds(200);
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try { return await operation(); }
            catch (OsrmUnavailableException)
            {
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
            }
        }
        return await operation();
    }

    private sealed class OsrmRouteResponse
    {
        public string Code { get; set; } = string.Empty;
        public List<OsrmRoute>? Routes { get; set; }
    }
    private sealed class OsrmRoute
    {
        public double Distance { get; set; }
        public double Duration { get; set; }
        public string Geometry { get; set; } = string.Empty;
    }
    private sealed class OsrmTableResponse
    {
        public string Code { get; set; } = string.Empty;
        public List<List<double?>>? Durations { get; set; }
    }
    private sealed class OsrmNearestResponse
    {
        public string Code { get; set; } = string.Empty;
        public List<OsrmWaypoint>? Waypoints { get; set; }
    }
    private sealed class OsrmWaypoint
    {
        public List<double>? Location { get; set; }
    }
}
