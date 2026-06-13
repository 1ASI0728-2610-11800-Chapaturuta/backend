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
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Reservations")]
public class ReservationsController(
    IReservationCommandService commandService,
    IReservationQueryService queryService) : ControllerBase
{
    [HttpPost]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(
        Summary = "Crear una reserva de asientos",
        Description = "Crea una nueva reserva en estado Pending, descuenta los asientos del Trip y registra un pago pendiente en el BC de Payments mediante la facade ACL.",
        OperationId = "CreateReservation")]
    [SwaggerResponse(StatusCodes.Status201Created, "Reserva creada correctamente", typeof(ReservationResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos inválidos o asientos insuficientes")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "No autorizado - token faltante o inválido")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Prohibido - rol insuficiente")]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationResource resource)
    {
        // No blanket try/catch: letting exceptions reach GlobalExceptionHandler preserves the
        // real status code (e.g. trip not found / insufficient seats -> 409, DB failure -> 500)
        // instead of masking every failure as a generic 400 that hides the underlying cause.
        var command = CreateReservationCommandFromResourceAssembler.ToCommandFromResource(resource);
        var reservation = await commandService.Handle(command);
        if (reservation == null) return BadRequest("Could not create reservation");
        var reservationResource = ReservationResourceFromEntityAssembler.ToResourceFromEntity(reservation);
        return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, reservationResource);
    }

    [HttpPost("{id}/confirm")]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(
        Summary = "Confirmar una reserva",
        Description = "Marca la reserva como Confirmed una vez que el pago asociado ha sido validado (por ejemplo, por el webhook del gateway).",
        OperationId = "ConfirmReservation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reserva confirmada", typeof(ReservationResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "La reserva no se pudo confirmar")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Reserva no encontrada")]
    public async Task<IActionResult> ConfirmReservation(int id)
    {
        try
        {
            var reservation = await commandService.Handle(new ConfirmReservationCommand(id));
            if (reservation == null) return NotFound();
            var resource = ReservationResourceFromEntityAssembler.ToResourceFromEntity(reservation);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(
        Summary = "Cancelar una reserva",
        Description = "Cancela una reserva, libera los asientos en el Trip y, si la reserva estaba Confirmed con un pago asociado, registra un reembolso en el BC de Payments.",
        OperationId = "CancelReservation")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reserva cancelada", typeof(ReservationResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "La reserva no se pudo cancelar")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Reserva no encontrada")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        try
        {
            var reservation = await commandService.Handle(new CancelReservationCommand(id));
            if (reservation == null) return NotFound();
            var resource = ReservationResourceFromEntityAssembler.ToResourceFromEntity(reservation);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener una reserva por ID",
        Description = "Devuelve los detalles de una reserva específica a partir de su identificador.",
        OperationId = "GetReservationById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reserva encontrada", typeof(ReservationResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Reserva no encontrada")]
    public async Task<IActionResult> GetReservationById(int id)
    {
        var reservation = await queryService.Handle(new GetReservationByIdQuery(id));
        if (reservation == null) return NotFound();
        var resource = ReservationResourceFromEntityAssembler.ToResourceFromEntity(reservation);
        return Ok(resource);
    }

    [HttpGet("by-user/{userId}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener reservas por usuario",
        Description = "Devuelve todas las reservas realizadas por un usuario específico, ordenadas de más reciente a más antigua.",
        OperationId = "GetReservationsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reservas encontradas", typeof(IEnumerable<ReservationResource>))]
    public async Task<IActionResult> GetReservationsByUser(int userId)
    {
        var reservations = await queryService.Handle(new GetReservationsByUserIdQuery(userId));
        var resources = reservations.Select(ReservationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("by-trip/{tripId}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener reservas por viaje",
        Description = "Devuelve todas las reservas asociadas a un Trip determinado.",
        OperationId = "GetReservationsByTrip")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reservas encontradas", typeof(IEnumerable<ReservationResource>))]
    public async Task<IActionResult> GetReservationsByTrip(int tripId)
    {
        var reservations = await queryService.Handle(new GetReservationsByTripIdQuery(tripId));
        var resources = reservations.Select(ReservationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("by-driver/{driverId}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener reservas por conductor",
        Description = "Devuelve todas las reservas correspondientes a viajes asignados a un conductor específico (join con Trip.FkIdDriver).",
        OperationId = "GetReservationsByDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Reservas encontradas", typeof(IEnumerable<ReservationResource>))]
    public async Task<IActionResult> GetReservationsByDriver(int driverId)
    {
        var reservations = await queryService.Handle(new GetReservationsByDriverIdQuery(driverId));
        var resources = reservations.Select(ReservationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
