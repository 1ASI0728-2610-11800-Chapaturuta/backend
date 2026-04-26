using Frock_backend.routes.Domain.Exceptions;
using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Model.Queries;
using Frock_backend.routes.Domain.Model.ValueObjects;
using Frock_backend.routes.Domain.Service;
using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.routes.Interface.REST.Transform;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.routes.Interface.REST
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Routes")]
    public class RoutesController(
        IRouteCommandService routeCommandService,
        IRouteQueryService routeQueryService,
        IOsrmRoutingService osrmRoutingService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Role.TransportManager, Role.Admin)]
        [SwaggerOperation(Summary = "Creates a new route.", OperationId = "Create route")]
        [SwaggerResponse(201, "The route was created", typeof(RouteAggregateResource))]
        [SwaggerResponse(400, "The route was not created")]
        [SwaggerResponse(401, "Unauthorized - token missing or invalid")]
        [SwaggerResponse(403, "Forbidden - insufficient role")]
        [SwaggerResponse(502, "OSRM routing service unavailable")]
        public async Task<ActionResult> CreateRoute([FromBody] CreateFullRouteResource resource)
        {
            if (resource == null) return BadRequest("Resource cannot be null.");

            var createRouteCommand = CreateFullRouteCommandFromResourceAssembler.toCommandFromResource(resource);
            try
            {
                var result = await routeCommandService.Handle(createRouteCommand);
                if (result is null) return BadRequest();
                return Ok(result);
            }
            catch (OsrmUnavailableException ex)
            {
                return StatusCode(502, new { error = "Routing service unavailable", detail = ex.Message });
            }
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all routes", OperationId = "GetAllRoutes")]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutes()
        {
            var routes = await routeQueryService.Handle(new GetAllRoutesQuery());
            if (routes == null || !routes.Any()) return NotFound("No routes found.");
            var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity).ToList();
            return Ok(resources);
        }

        [HttpGet("company/{FkIdCompany}")]
        [SwaggerOperation(Summary = "Get Routes By Company Id", OperationId = "GetRoutesByCompanyId")]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutes(int FkIdCompany)
        {
            var routes = await routeQueryService.Handle(new GetAllRoutesByFkCompanyIdQuery(FkIdCompany));
            if (routes == null || !routes.Any()) return NotFound("No routes found.");
            var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity).ToList();
            return Ok(resources);
        }

        [HttpGet("district/{FkIdDistrict}")]
        [SwaggerOperation(Summary = "Get Routes By District Id", OperationId = "GetRoutesByDistrictId")]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutesByDistrict(int FkIdDistrict)
        {
            var routes = await routeQueryService.Handle(new GetAllRoutesByFkDistrictIdQuery(FkIdDistrict));
            if (routes == null || !routes.Any()) return NotFound("No routes found.");
            var resources = routes.Select(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity).ToList();
            return Ok(resources);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get Route By Id", OperationId = "GetRouteById")]
        [SwaggerResponse(200, "The route was retrieved", typeof(RouteAggregateResource))]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteAggregateResource>> GetRouteById(int id)
        {
            var route = await routeQueryService.Handle(new GetRouteByIdQuery(id));
            if (route == null) return NotFound("Route not found.");
            return Ok(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(route));
        }

        [HttpPut("{id}")]
        [Authorize(Role.TransportManager, Role.Admin)]
        [SwaggerOperation(Summary = "Update Route", OperationId = "UpdateRoute")]
        [SwaggerResponse(200, "The route was updated", typeof(RouteAggregateResource))]
        [SwaggerResponse(401, "Unauthorized - token missing or invalid")]
        [SwaggerResponse(403, "Forbidden - insufficient role")]
        [SwaggerResponse(404, "Route not found")]
        [SwaggerResponse(502, "OSRM routing service unavailable")]
        public async Task<ActionResult<RouteAggregateResource>> UpdateRoute(int id, [FromBody] UpdateRouteResource resource)
        {
            if (resource == null) return BadRequest("Resource cannot be null.");
            var command = UpdateRouteCommandFromResourceAssembler.toCommandFromResource(resource);
            try
            {
                var result = await routeCommandService.Handle(id, command);
                if (result is null) return NotFound("Route not found.");
                return Ok(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(result));
            }
            catch (OsrmUnavailableException ex)
            {
                return StatusCode(502, new { error = "Routing service unavailable", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Role.TransportManager, Role.Admin)]
        [SwaggerOperation(Summary = "Delete Route", OperationId = "DeleteRoute")]
        [SwaggerResponse(204, "The route was deleted")]
        [SwaggerResponse(401, "Unauthorized - token missing or invalid")]
        [SwaggerResponse(403, "Forbidden - insufficient role")]
        [SwaggerResponse(404, "Route not found")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            await routeCommandService.Handle(new DeleteRouteCommand(id));
            return NoContent();
        }

        [HttpPatch("{id}/toggle-availability")]
        [Authorize(Role.TransportManager, Role.Admin)]
        [SwaggerOperation(Summary = "Toggle Route Availability", OperationId = "ToggleRouteAvailability")]
        [SwaggerResponse(200, "The route availability was toggled", typeof(RouteAggregateResource))]
        [SwaggerResponse(401, "Unauthorized - token missing or invalid")]
        [SwaggerResponse(403, "Forbidden - insufficient role")]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteAggregateResource>> ToggleRouteAvailability(int id)
        {
            var result = await routeCommandService.ToggleAvailability(id);
            if (result is null) return NotFound("Route not found.");
            return Ok(RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(result));
        }

        // ── NEW ENDPOINTS ──────────────────────────────────────────────────────────

        [HttpPost("preview")]
        [SwaggerOperation(
            Summary = "Preview route geometry without persisting",
            Description = "Calculates distance, duration and geometry via OSRM without saving to DB.",
            OperationId = "PreviewRoute")]
        [SwaggerResponse(200, "Route preview calculated", typeof(RoutePreviewResultResource))]
        [SwaggerResponse(400, "At least 2 coordinates required")]
        [SwaggerResponse(502, "OSRM routing service unavailable")]
        public async Task<ActionResult<RoutePreviewResultResource>> PreviewRoute([FromBody] RoutePreviewResource resource)
        {
            if (resource?.Coordinates == null || resource.Coordinates.Count < 2)
                return BadRequest("At least 2 coordinates required.");

            var waypoints = resource.Coordinates
                .Select(c => new Coordinate(c.Latitude, c.Longitude))
                .ToList();

            try
            {
                var result = await osrmRoutingService.RouteAsync(waypoints);
                return Ok(new RoutePreviewResultResource(result.DistanceMeters, result.DurationSeconds, result.Geometry));
            }
            catch (OsrmUnavailableException ex)
            {
                return StatusCode(502, new { error = "Routing service unavailable", detail = ex.Message });
            }
        }

        [HttpGet("{id}/geometry")]
        [SwaggerOperation(
            Summary = "Get route geometry",
            Description = "Returns only the encoded polyline of a persisted route.",
            OperationId = "GetRouteGeometry")]
        [SwaggerResponse(200, "Geometry retrieved", typeof(RouteGeometryResource))]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteGeometryResource>> GetRouteGeometry(int id)
        {
            var route = await routeQueryService.Handle(new GetRouteByIdQuery(id));
            if (route == null) return NotFound("Route not found.");
            return Ok(new RouteGeometryResource(route.Id, route.Geometry));
        }

        [HttpGet("{id}/eta")]
        [SwaggerOperation(
            Summary = "Get ETA from current position to route destination",
            Description = "Given current lat/lng (e.g. bus position), calculates ETA to the last stop of the route via OSRM.",
            OperationId = "GetRouteEta")]
        [SwaggerResponse(200, "ETA calculated", typeof(RouteEtaResource))]
        [SwaggerResponse(404, "Route not found or missing coordinates")]
        [SwaggerResponse(502, "OSRM routing service unavailable")]
        public async Task<ActionResult<RouteEtaResource>> GetRouteEta(int id, [FromQuery] double lat, [FromQuery] double lng)
        {
            var route = await routeQueryService.Handle(new GetRouteByIdQuery(id));
            if (route == null) return NotFound("Route not found.");

            var lastStop = route.Stops.LastOrDefault()?.Stop;
            if (lastStop?.Latitude == null || lastStop.Longitude == null)
                return NotFound("Route destination has no coordinates.");

            var waypoints = new[]
            {
                new Coordinate(lat, lng),
                new Coordinate(lastStop.Latitude.Value, lastStop.Longitude.Value)
            };

            try
            {
                var result = await osrmRoutingService.RouteAsync(waypoints);
                return Ok(new RouteEtaResource(id, result.DurationSeconds, result.DurationSeconds / 60.0));
            }
            catch (OsrmUnavailableException ex)
            {
                return StatusCode(502, new { error = "Routing service unavailable", detail = ex.Message });
            }
        }

        // ── FASE 2 STUBS ───────────────────────────────────────────────────────────

        [HttpPost("suggest-stops")]
        [SwaggerOperation(
            Summary = "[Fase 2] Suggest stops using Overpass + OSRM",
            Description = "NOT IMPLEMENTED — Fase 2. Will suggest intermediate stops between origin and destination using Overpass API and OSRM.",
            OperationId = "SuggestStops")]
        [SwaggerResponse(501, "Not implemented — Fase 2")]
        [Obsolete("Fase 2 — not implemented yet")]
        public IActionResult SuggestStops()
        {
            return StatusCode(501, new { message = "Fase 2 — not implemented yet" });
        }
    }
}
