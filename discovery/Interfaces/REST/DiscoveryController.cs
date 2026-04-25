using Frock_backend.Discovery.Domain.Model.Queries;
using Frock_backend.Discovery.Domain.Services;
using Frock_backend.Discovery.Interfaces.REST.Resources;
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
    [SwaggerOperation(Summary = "Search routes by origin/destination", OperationId = "SearchRoutes")]
    public async Task<IActionResult> SearchRoutes([FromQuery] string? origin, [FromQuery] string? destination, [FromQuery] string? date)
    {
        var query = new SearchRoutesQuery(origin, destination, date);
        var routes = await queryService.Handle(query);
        var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("nearby")]
    [SwaggerOperation(Summary = "Find nearby stops by coordinates", OperationId = "GetNearbyStops")]
    public async Task<IActionResult> GetNearbyStops([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 2.0)
    {
        var query = new GetNearbyStopsQuery(lat, lng, radius);
        var stops = await queryService.Handle(query);
        var resources = stops.Select(s => new NearbyStopResource(s.Id, s.Name, s.Address, s.Latitude, s.Longitude, s.FkIdCompany, s.FkIdDistrict));
        return Ok(resources);
    }

    [HttpGet("popular")]
    [SwaggerOperation(Summary = "Get popular routes based on trip count", OperationId = "GetPopularRoutes")]
    public async Task<IActionResult> GetPopularRoutes([FromQuery] int limit = 10)
    {
        var query = new GetPopularRoutesQuery(limit);
        var routes = await queryService.Handle(query);
        var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("analytics/demand")]
    [SwaggerOperation(Summary = "Get demand analytics by district and period", OperationId = "GetDemandAnalytics")]
    public async Task<IActionResult> GetDemandAnalytics([FromQuery] int? districtId, [FromQuery] string? period)
    {
        var query = new GetDemandAnalyticsQuery(districtId, period);
        var result = await queryService.Handle(query);
        return Ok(result);
    }
}
