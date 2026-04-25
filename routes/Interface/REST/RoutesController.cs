using Frock_backend.routes.Domain.Model.Commands;
using Frock_backend.routes.Domain.Model.Queries;
using Frock_backend.routes.Domain.Service;
using Frock_backend.routes.Interface.REST.Resources;
using Frock_backend.routes.Interface.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
namespace Frock_backend.routes.Interface.REST
{
    /// <summary>
    /// Routes controller.
    /// </summary>
    /// <param name="routeCommandService">The Route Command Service</param>
    /// <param name="">The Route Query Service</param>
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Routes")]
    public class RoutesController(IRouteCommandService routeCommandService, IRouteQueryService routeQueryService) : ControllerBase
    {
        /// <summary>
        /// Creates a new route.
        /// </summary>
        /// <param name="resource">The CreateStopResource resource</param>
        /// <returns>
        /// A response as an action result containing the created stop, or bad request if the stop was not created.
        /// </returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Creates a new route.",
            Description = "Creates a new route with a given parameters",
            OperationId = "Create route"
            )]
        [SwaggerResponse(201, "The route was created", typeof(CreateFullRouteResource))]
        [SwaggerResponse(400, "The route was not created")]
        public async Task<ActionResult> CreateRoute([FromBody] CreateFullRouteResource resource)
        {
            if (resource == null)
            {
                return BadRequest("Resource cannot be null.");
            }
            var createRouteCommand = CreateFullRouteCommandFromResourceAssembler.toCommandFromResource(resource);
            var result = await routeCommandService.Handle(createRouteCommand);
            if (result is null) return BadRequest();
            return Ok(result);
        }

        //Get all routes
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all routes",
            Description = "Get all routes in the system",
            OperationId = "GetAllRoutes"
            )]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutes()
        {
            GetAllRoutesQuery query = new GetAllRoutesQuery();
            var routes = await routeQueryService.Handle(query); // Assuming this method exists in the service
            if (routes == null || !routes.Any())
            {
                return NotFound("No routes found.");
            }
            var resources = routes.Select((routeAggregate) => RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(routeAggregate)).ToList();
            return Ok(resources);
        }

        /// <summary>
        /// Gets all routes by FkIdCompany.
        /// </summary>
        /// <param name="resource">The GetStopsBy resource</param>
        /// <returns>
        /// A response as an action result containing the Get, or bad request if the stop was not created.
        /// </returns>
        [HttpGet("company/{FkIdCompany}")]
        [SwaggerOperation(
            Summary = "Get Routes By Company Id",
            Description = "Get routes by Company Id",
            OperationId = "GetRoutesByCompanyId"
            )]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutes(int FkIdCompany)
        {
            GetAllRoutesByFkCompanyIdQuery query = new GetAllRoutesByFkCompanyIdQuery(FkIdCompany);
            var routes = await routeQueryService.Handle(query); // Assuming this method exists in the service
            if (routes == null || !routes.Any())
            {
                return NotFound("No routes found.");
            }

            var resources = routes.Select((routeAggregate) => RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(routeAggregate)).ToList();

            return Ok(resources);
        }

        /// <summary>
        /// Gets all routes by FkIdDistrict.
        /// 
        [HttpGet("district/{FkIdDistrict}")]
        [SwaggerOperation(
            Summary = "Get Routes By District Id",
            Description = "Get routes by District Id",
            OperationId = "GetRoutesByDistrictId"
            )]
        [SwaggerResponse(200, "The routes were retrieved", typeof(IEnumerable<RouteAggregateResource>))]
        [SwaggerResponse(404, "No routes found")]
        public async Task<ActionResult<IEnumerable<RouteAggregateResource>>> GetAllRoutesByDistrict(int FkIdDistrict)
        {
            GetAllRoutesByFkDistrictIdQuery query = new GetAllRoutesByFkDistrictIdQuery(FkIdDistrict);
            var routes = await routeQueryService.Handle(query); // Assuming this method exists in the service
            if (routes == null || !routes.Any())
            {
                return NotFound("No routes found.");
            }
            var resources = routes.Select((routeAggregate) => RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(routeAggregate)).ToList();
            return Ok(resources);
        }

        //Get route by id
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get Route By Id",
            Description = "Get route by Id",
            OperationId = "GetRouteById"
            )]
        [SwaggerResponse(200, "The route was retrieved", typeof(RouteAggregateResource))]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteAggregateResource>> GetRouteById(int id)
        {
            GetRouteByIdQuery query = new GetRouteByIdQuery(id);
            var route = await routeQueryService.Handle(query); // Assuming this method exists in the service
            if (route == null)
            {
                return NotFound("Route not found.");
            }
            var resource = RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(route);
            return Ok(resource);

        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update Route",
            Description = "Update a route by Id",
            OperationId = "UpdateRoute"
            )]
        [SwaggerResponse(200, "The route was updated", typeof(RouteAggregateResource))]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteAggregateResource>> UpdateRoute(int id, [FromBody] UpdateRouteResource resource)
        {
            if (resource == null)
            {
                return BadRequest("Resource cannot be null or ID mismatch.");
            }
            var updateRouteCommand = UpdateRouteCommandFromResourceAssembler.toCommandFromResource(resource);
            var result = await routeCommandService.Handle(id, updateRouteCommand);
            if (result is null) return NotFound("Route not found.");
            var updatedResource = RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(result);
            return Ok(updatedResource);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete Route",
            Description = "Delete a route by Id",
            OperationId = "DeleteRoute"
            )]
        [SwaggerResponse(204, "The route was deleted")]
        [SwaggerResponse(404, "Route not found")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var command = new DeleteRouteCommand(id);
            await routeCommandService.Handle(command);
            return NoContent();
        }

        [HttpPatch("{id}/toggle-availability")]
        [SwaggerOperation(
            Summary = "Toggle Route Availability",
            Description = "Toggle the active/inactive state of a route",
            OperationId = "ToggleRouteAvailability"
            )]
        [SwaggerResponse(200, "The route availability was toggled", typeof(RouteAggregateResource))]
        [SwaggerResponse(404, "Route not found")]
        public async Task<ActionResult<RouteAggregateResource>> ToggleRouteAvailability(int id)
        {
            var result = await routeCommandService.ToggleAvailability(id);
            if (result is null) return NotFound("Route not found.");
            var resource = RouteAggregateResourceFromResourceAssembler.ToResourceFromEntity(result);
            return Ok(resource);
        }
    }
}
