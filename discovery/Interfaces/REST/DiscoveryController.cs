using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.Discovery.Interfaces.REST.Resources;
using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.routes.Interface.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Discovery.Interfaces.REST;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Discovery")]
public class DiscoveryController(IDiscoveryQueryService queryService) : ControllerBase
{
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Search routes by origin/destination",
        Description = "Returns matching routes. When both origin and destination are provided, includes OSRM estimated distance and duration for each match.",
        OperationId = "SearchRoutes")]
    [SwaggerResponse(200, "Matching routes with optional OSRM estimates")]
    public async Task<IActionResult> SearchRoutes(
        [FromQuery] string? origin,
        [FromQuery] string? destination,
        [FromQuery] string? date)
    {
        var query = new SearchRoutesQuery(origin, destination, date);
        var results = await queryService.Handle(query);
        var resources = results.Select(r => new
        {
            route = RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(r.Route),
            estimatedDistanceMeters = r.EstimatedDistanceMeters,
            estimatedDurationSeconds = r.EstimatedDurationSeconds
        });
        return Ok(resources);
    }

    [HttpGet("nearby")]
    [SwaggerOperation(
        Summary = "Find nearby stops by coordinates",
        Description = "Returns stops within radius. Add ?useRoadDistance=true to sort by road distance via OSRM instead of straight-line Haversine.",
        OperationId = "GetNearbyStops")]
    [SwaggerResponse(200, "Nearby stops ordered by distance", typeof(IEnumerable<NearbyStopResource>))]
    public async Task<IActionResult> GetNearbyStops(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 2.0,
        [FromQuery] bool useRoadDistance = false)
    {
        var query = new GetNearbyStopsQuery(lat, lng, radius);
        var stops = await queryService.Handle(query, useRoadDistance);
        var resources = stops.Select(s => new NearbyStopResource(s.Id, s.Name, s.Address, s.Latitude, s.Longitude, s.FkIdCompany, s.FkIdDistrict));
        return Ok(resources);
    }

    [HttpGet("popular")]
    [SwaggerOperation(Summary = "Get popular routes based on trip count", OperationId = "GetPopularRoutes")]
    [SwaggerResponse(200, "Popular routes", typeof(IEnumerable<RouteAggregateResource>))]
    public async Task<IActionResult> GetPopularRoutes([FromQuery] int limit = 10)
    {
        var query = new GetPopularRoutesQuery(limit);
        var routes = await queryService.Handle(query);
        var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("analytics/demand")]
    [SwaggerOperation(Summary = "Get demand analytics by district and period", OperationId = "GetDemandAnalytics")]
    [SwaggerResponse(200, "Demand analytics grouped by hour and day of week")]
    public async Task<IActionResult> GetDemandAnalytics([FromQuery] int? districtId, [FromQuery] string? period)
    {
        var query = new GetDemandAnalyticsQuery(districtId, period);
        var result = await queryService.Handle(query);
        return Ok(result);
    }

    // ── FASE 2 STUBS ───────────────────────────────────────────────────────────

    [HttpGet("pois")]
    [SwaggerOperation(
        Summary = "[Fase 2] Get POIs near coordinates via Overpass API",
        Description = "NOT IMPLEMENTED — Fase 2. Will return real OSM POIs (pharmacies, hospitals, gas stations, restaurants) within given radius.",
        OperationId = "GetNearbyPois")]
    [SwaggerResponse(501, "Not implemented — Fase 2")]
    [Obsolete("Fase 2 — not implemented yet")]
    public IActionResult GetNearbyPois(
        [FromQuery] double lat, [FromQuery] double lng,
        [FromQuery] double radius = 0.5, [FromQuery] string? type = null)
    {
        return StatusCode(501, new { message = "Fase 2 — not implemented yet" });
    }

    [HttpGet("pois/along-route")]
    [SwaggerOperation(
        Summary = "[Fase 2] Get POIs along a route buffer via Overpass API",
        Description = "NOT IMPLEMENTED — Fase 2. Will return POIs within a buffer around an existing route.",
        OperationId = "GetPoisAlongRoute")]
    [SwaggerResponse(501, "Not implemented — Fase 2")]
    [Obsolete("Fase 2 — not implemented yet")]
    public IActionResult GetPoisAlongRoute(
        [FromQuery] int routeId, [FromQuery] string? type = null, [FromQuery] double buffer = 0.5)
    {
        return StatusCode(501, new { message = "Fase 2 — not implemented yet" });
    }
}
