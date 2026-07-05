using Frock_backend.Trips.Domain.Model.Commands;
using Frock_backend.Trips.Domain.Model.Queries;
using Frock_backend.Trips.Domain.Services;
using Frock_backend.Trips.Interfaces.REST.Resources;
using Frock_backend.Trips.Interfaces.REST.Transform;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Frock_backend.IAM.Domain.Model.Aggregates;
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
    // Authenticated user id, injected by the IAM middleware into HttpContext.Items["User"].
    private int? CurrentUserId => (HttpContext.Items["User"] as User)?.Id;

    [HttpPost]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(Summary = "Register a trip", OperationId = "CreateTrip")]
    [SwaggerResponse(StatusCodes.Status201Created, "Trip created", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden - insufficient role")]
    public async Task<IActionResult> CreateTrip([FromBody] CreateTripResource resource)
    {
        // No blanket try/catch: letting exceptions reach GlobalExceptionHandler preserves the
        // real status code (e.g. a DB save failure -> 500 with the cause logged) instead of
        // masking every failure as a generic 400 that hides the underlying error.
        var command = new CreateTripCommand(resource.FkIdUser, resource.FkIdDriver, resource.FkIdRoute, resource.FkIdOriginStop, resource.FkIdDestinationStop, resource.Price, resource.AvailableSeats, resource.StartTime);
        var trip = await commandService.Handle(command);
        if (trip == null) return BadRequest("Could not create trip");
        var tripResource = TripResourceFromEntityAssembler.ToResourceFromEntity(trip);
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, tripResource);
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

    [HttpGet("user/{userId}/history")]
    [Authorize]
    [SwaggerOperation(Summary = "Get enriched trip history for a passenger (resolved names)", OperationId = "GetTripHistoryByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip history found", typeof(IEnumerable<TripHistoryResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    public async Task<IActionResult> GetTripHistoryByUser(int userId)
    {
        var views = await queryService.Handle(new GetTripHistoryByUserIdQuery(userId));
        var resources = views.Select(TripHistoryResourceFromViewAssembler.ToResourceFromView);
        return Ok(resources);
    }

    [HttpGet("driver/{driverId}/history")]
    [Authorize]
    [SwaggerOperation(Summary = "Get enriched trip history for a driver (resolved names)", OperationId = "GetTripHistoryByDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip history found", typeof(IEnumerable<TripHistoryResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    public async Task<IActionResult> GetTripHistoryByDriver(int driverId)
    {
        var views = await queryService.Handle(new GetTripHistoryByDriverIdQuery(driverId));
        var resources = views.Select(TripHistoryResourceFromViewAssembler.ToResourceFromView);
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

    [HttpPost("publish")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Publish a shared trip with seat capacity", OperationId = "PublishTrip")]
    [SwaggerResponse(StatusCodes.Status201Created, "Trip published", typeof(TripResource))]
    public async Task<IActionResult> PublishTrip([FromBody] PublishTripResource resource)
    {
        // Reuses CreateTripCommand: a published trip is just a trip with a driver and a seat
        // capacity (AvailableSeats), which passengers then draw down by reserving.
        var command = new CreateTripCommand(
            resource.FkIdUser, resource.FkIdDriver, resource.FkIdRoute,
            resource.FkIdOriginStop, resource.FkIdDestinationStop, resource.Price, resource.Seats, resource.StartTime);
        var trip = await commandService.Handle(command);
        if (trip == null) return BadRequest("Could not publish trip");
        var tripResource = TripResourceFromEntityAssembler.ToResourceFromEntity(trip);
        return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, tripResource);
    }

    [HttpGet("joinable")]
    [Authorize]
    [SwaggerOperation(Summary = "Get published trips a passenger can still join (pending + free seats)", OperationId = "GetJoinableTrips")]
    [SwaggerResponse(StatusCodes.Status200OK, "Joinable trips found", typeof(IEnumerable<TripHistoryResource>))]
    public async Task<IActionResult> GetJoinableTrips([FromQuery] int? routeId)
    {
        var views = await queryService.Handle(new GetJoinableTripsQuery(routeId));
        var resources = views.Select(TripHistoryResourceFromViewAssembler.ToResourceFromView);
        return Ok(resources);
    }

    [HttpGet("available")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Get pending trips with no driver assigned (claimable pool)", OperationId = "GetAvailableTrips")]
    [SwaggerResponse(StatusCodes.Status200OK, "Available trips found", typeof(IEnumerable<TripHistoryResource>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    public async Task<IActionResult> GetAvailableTrips()
    {
        if (CurrentUserId is null) return Unauthorized();
        var views = await queryService.Handle(new GetAvailableTripsQuery(CurrentUserId.Value));
        var resources = views.Select(TripHistoryResourceFromViewAssembler.ToResourceFromView);
        return Ok(resources);
    }

    [HttpPost("{id}/assign-driver")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Assign (claim) a driver to a trip", OperationId = "AssignDriverToTrip")]
    [SwaggerResponse(StatusCodes.Status200OK, "Driver assigned", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Trip not found")]
    public async Task<IActionResult> AssignDriver(int id, [FromBody] AssignDriverResource resource)
    {
        try
        {
            var trip = await commandService.Handle(new AssignDriverToTripCommand(id, resource.DriverId));
            if (trip == null) return NotFound();
            return Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/start")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Start a trip", OperationId = "StartTrip")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip started", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Trip not found")]
    public async Task<IActionResult> StartTrip(int id)
    {
        if (CurrentUserId is null) return Unauthorized();
        try
        {
            var trip = await commandService.Handle(new StartTripCommand(id, CurrentUserId.Value));
            if (trip == null) return NotFound();
            return Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Complete a trip", OperationId = "CompleteTrip")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip completed", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Trip not found")]
    public async Task<IActionResult> CompleteTrip(int id)
    {
        try
        {
            var trip = await commandService.Handle(new CompleteTripCommand(id));
            if (trip == null) return NotFound();
            return Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Role.Driver, Role.Admin)]
    [SwaggerOperation(Summary = "Cancel a trip", OperationId = "CancelTrip")]
    [SwaggerResponse(StatusCodes.Status200OK, "Trip cancelled", typeof(TripResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Trip not found")]
    public async Task<IActionResult> CancelTrip(int id)
    {
        try
        {
            var trip = await commandService.Handle(new CancelTripCommand(id));
            if (trip == null) return NotFound();
            return Ok(TripResourceFromEntityAssembler.ToResourceFromEntity(trip));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
