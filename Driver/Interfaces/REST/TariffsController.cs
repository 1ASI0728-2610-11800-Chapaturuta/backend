using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.Queries;
using Frock_backend.Driver.Domain.Services;
using Frock_backend.Driver.Interfaces.REST.Resources;
using Frock_backend.Driver.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Driver.Interfaces.REST;

[ApiController]
[Route("api/v1/tariffs")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Tariffs")]
public class TariffsController(
    ITariffCommandService commandService,
    ITariffQueryService queryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a tariff", OperationId = "CreateTariff")]
    [SwaggerResponse(StatusCodes.Status201Created, "Tariff created", typeof(TariffResource))]
    public async Task<IActionResult> CreateTariff([FromBody] CreateTariffResource resource)
    {
        try
        {
            var command = CreateTariffCommandFromResourceAssembler.ToCommandFromResource(resource);
            var tariff = await commandService.Handle(command);
            if (tariff == null) return BadRequest("Could not create tariff");
            var tariffResource = TariffResourceFromEntityAssembler.ToResourceFromEntity(tariff);
            return CreatedAtAction(nameof(GetTariffByDriver), new { driverId = tariff.FkIdDriver }, tariffResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("by-driver/{driverId:int}")]
    [SwaggerOperation(Summary = "Get the active tariff for a driver", OperationId = "GetTariffByDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Tariff found", typeof(TariffResource))]
    public async Task<IActionResult> GetTariffByDriver(int driverId)
    {
        var tariff = await queryService.Handle(new GetTariffByDriverIdQuery(driverId));
        if (tariff == null) return NotFound();
        return Ok(TariffResourceFromEntityAssembler.ToResourceFromEntity(tariff));
    }

    [HttpPatch("{id:int}")]
    [SwaggerOperation(Summary = "Update a tariff", OperationId = "UpdateTariff")]
    [SwaggerResponse(StatusCodes.Status200OK, "Tariff updated", typeof(TariffResource))]
    public async Task<IActionResult> UpdateTariff(int id, [FromBody] UpdateTariffResource resource)
    {
        try
        {
            var command = new UpdateTariffCommand(
                id,
                resource.BaseFare,
                resource.PricePerKm,
                resource.PricePerMinute,
                resource.MinFare,
                resource.AvailableDays ?? Enumerable.Empty<DayOfWeek>());
            var tariff = await commandService.Handle(command);
            if (tariff == null) return NotFound();
            return Ok(TariffResourceFromEntityAssembler.ToResourceFromEntity(tariff));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/route-durations")]
    [SwaggerOperation(Summary = "Set the estimated duration for a route under a tariff", OperationId = "SetRouteDuration")]
    [SwaggerResponse(StatusCodes.Status200OK, "Route duration saved", typeof(RouteDurationResource))]
    public async Task<IActionResult> SetRouteDuration(int id, [FromBody] SetRouteDurationResource resource)
    {
        try
        {
            var command = new SetRouteDurationCommand(id, resource.FkIdRoute, resource.EstimatedMinutes);
            var routeDuration = await commandService.Handle(command);
            if (routeDuration == null) return BadRequest("Could not save route duration");
            return Ok(RouteDurationResourceFromEntityAssembler.ToResourceFromEntity(routeDuration));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{driverId:int}/route-durations/{routeId:int}")]
    [SwaggerOperation(Summary = "Get the estimated duration for a driver/route pair", OperationId = "GetRouteDurationByDriverAndRoute")]
    [SwaggerResponse(StatusCodes.Status200OK, "Route duration found", typeof(RouteDurationResource))]
    public async Task<IActionResult> GetRouteDurationByDriverAndRoute(int driverId, int routeId)
    {
        var routeDuration = await queryService.Handle(new GetRouteDurationByDriverAndRouteQuery(driverId, routeId));
        if (routeDuration == null) return NotFound();
        return Ok(RouteDurationResourceFromEntityAssembler.ToResourceFromEntity(routeDuration));
    }
}
