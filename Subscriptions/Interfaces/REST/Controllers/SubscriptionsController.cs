using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Model.Queries;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;
using Frock_backend.Subscriptions.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionsController(
    ISubscriptionCommandService subscriptionCommandService,
    ISubscriptionQueryService subscriptionQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        Summary = "Suscribir un usuario a un plan",
        Description = "Crea una suscripcion para un usuario. Para planes Free se activa inmediatamente; para planes Premium se registra un pago pendiente.",
        OperationId = "SubscribeToPlan")]
    [SwaggerResponse(StatusCodes.Status201Created, "Suscripcion creada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos invalidos o plan no disponible")]
    public async Task<IActionResult> SubscribeToPlan([FromBody] SubscribeToPlanResource resource)
    {
        try
        {
            var command = SubscribeToPlanCommandFromResourceAssembler.ToCommandFromResource(resource);
            var subscription = await subscriptionCommandService.Handle(command);
            if (subscription == null) return BadRequest("Could not subscribe to plan");
            var subscriptionResource = SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription);
            return CreatedAtAction(nameof(GetActiveSubscriptionByUser), new { userId = subscription.FkIdUser }, subscriptionResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancel")]
    [SwaggerOperation(
        Summary = "Cancelar una suscripcion",
        Description = "Cancela la suscripcion indicada. Si es Premium y esta dentro de la ventana de 7 dias desde la activacion, se registra un reembolso automatico.",
        OperationId = "CancelSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripcion cancelada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripcion no encontrada")]
    public async Task<IActionResult> CancelSubscription(int id)
    {
        try
        {
            var command = new CancelSubscriptionCommand(id);
            var subscription = await subscriptionCommandService.Handle(command);
            if (subscription == null) return NotFound();
            return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/renew")]
    [SwaggerOperation(
        Summary = "Renovar una suscripcion",
        Description = "Renueva una suscripcion existente extendiendo el ciclo. Los planes Premium generan un pago pendiente con el metodo indicado.",
        OperationId = "RenewSubscription")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripcion renovada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Suscripcion no encontrada")]
    public async Task<IActionResult> RenewSubscription(int id, [FromBody] RenewSubscriptionResource resource)
    {
        try
        {
            var command = new RenewSubscriptionCommand(id, resource.PaymentMethod);
            var subscription = await subscriptionCommandService.Handle(command);
            if (subscription == null) return NotFound();
            return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("active/by-user/{userId:int}")]
    [SwaggerOperation(
        Summary = "Obtener la suscripcion activa de un usuario",
        Description = "Devuelve la suscripcion vigente (estado Active y no vencida) de un usuario, si existe.",
        OperationId = "GetActiveSubscriptionByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Suscripcion activa encontrada", typeof(SubscriptionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "El usuario no tiene suscripcion activa")]
    public async Task<IActionResult> GetActiveSubscriptionByUser(int userId)
    {
        var subscription = await subscriptionQueryService.Handle(new GetActiveSubscriptionByUserIdQuery(userId));
        if (subscription == null) return NotFound();
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
    }

    [HttpGet("history/by-user/{userId:int}")]
    [SwaggerOperation(
        Summary = "Obtener historial de suscripciones de un usuario",
        Description = "Devuelve todas las suscripciones (activas, expiradas, canceladas y pendientes) asociadas al usuario.",
        OperationId = "GetSubscriptionHistoryByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Historial de suscripciones", typeof(IEnumerable<SubscriptionResource>))]
    public async Task<IActionResult> GetSubscriptionHistoryByUser(int userId)
    {
        var subscriptions = await subscriptionQueryService.Handle(new GetSubscriptionHistoryByUserIdQuery(userId));
        var resources = subscriptions.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
