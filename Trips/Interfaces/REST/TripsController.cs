using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.Trips.Interfaces.REST.Resources;
using Frock_backend.Trips.Interfaces.REST.Transform;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Trips.Interfaces.REST;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Trips")]
public class TripsController(ITripCommandService commandService, ITripQueryService queryService) : ControllerBase
{
    [HttpPost]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(Summary = "Register a trip", OperationId = "CreateTrip")]
    [SwaggerResponse(StatusCodes.Status201Created, "Trip created", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden - insufficient role")]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripResource resource)
    {
        try
        {
            var command = new CreateTripCommand(resource.FkIdUser, resource.FkIdDriver, resource.FkIdRoute, resource.FkIdOriginStop, resource.FkIdDestinationStop, resource.Price);
            var trip = await commandService.Handle(command);
            if (trip == null) return BadRequest("Could not create trip");
            var tripResource = TripResourceFromEntityAssembler.ToResourceFromEntity(trip);
            return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, tripResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [Authorize]
    [SwaggerOperation(Summary = "Get trip history for a passenger", OperationId = "GetTripsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trips found", typeof(IEnumerable<TripResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    public async Task<IActionResult> GetTripsByUser(int userId)
    {
        var query = new GetTripsByUserIdQuery(userId);
        var trips = await queryService.Handle(query);
        var resources = trips.Select(TripResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("driver/{driverId}")]
    [Authorize]
    [SwaggerOperation(Summary = "Get trip history for a driver", OperationId = "GetTripsByDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trips found", typeof(IEnumerable<TripResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    public async Task<IActionResult> GetTripsByDriver(int driverId)
    {
        var query = new GetTripsByDriverIdQuery(driverId);
        var trips = await queryService.Handle(query);
        var resources = trips.Select(TripResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Get trip by ID", OperationId = "GetTripById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip found", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Trip not found")]
    public async Task<IActionResult> GetTripById(int id)
    {
        var query = new GetTripByIdQuery(id);
        var trip = await queryService.Handle(query);
        if (trip == null) return NotFound();
        var resource = TripResourceFromEntityAssembler.ToResourceFromEntity(trip);
        return Ok(resource);
    }
}
