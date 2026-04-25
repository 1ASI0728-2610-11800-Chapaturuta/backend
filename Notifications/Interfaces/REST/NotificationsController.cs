using Frock_backend.Notifications.Domain.Model.Commands;
using Frock_backend.Notifications.Domain.Model.Queries;
using Frock_backend.Notifications.Domain.Services;
using Frock_backend.Notifications.Interfaces.REST.Resources;
using Frock_backend.Notifications.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Notifications.Interfaces.REST;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Notifications")]
public class NotificationsController(INotificationCommandService commandService, INotificationQueryService queryService) : ControllerBase
{
    [HttpGet("user/{userId}")]
    [SwaggerOperation(Summary = "Get notifications for a user", OperationId = "GetNotificationsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Notifications found", typeof(IEnumerable<NotificationResource>))]
    public async Task<IActionResult> GetNotificationsByUser(int userId)
    {
        var query = new GetNotificationsByUserIdQuery(userId);
        var notifications = await queryService.Handle(query);
        var resources = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPut("{id}/read")]
    [SwaggerOperation(Summary = "Mark notification as read", OperationId = "MarkNotificationRead")]
    [SwaggerResponse(StatusCodes.Status200OK, "Notification marked as read", typeof(NotificationResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Notification not found")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var command = new MarkNotificationReadCommand(id);
        var notification = await commandService.Handle(command);
        if (notification == null) return NotFound();
        var resource = NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification);
        return Ok(resource);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a notification", OperationId = "DeleteNotification")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Notification deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Notification not found")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        try
        {
            var command = new DeleteNotificationCommand(id);
            await commandService.Handle(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
