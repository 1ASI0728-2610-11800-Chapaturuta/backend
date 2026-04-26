using Frock_backend.Collections.Domain.Model.Commands;
using Frock_backend.Collections.Domain.Model.Queries;
using Frock_backend.Collections.Domain.Services;
using Frock_backend.Collections.Interfaces.REST.Resources;
using Frock_backend.Collections.Interfaces.REST.Transform;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Collections.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Collections")]
public class CollectionsController(ICollectionCommandService commandService, ICollectionQueryService queryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a collection", OperationId = "CreateCollection")]
    [SwaggerResponse(StatusCodes.Status201Created, "Collection created", typeof(CollectionResource))]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionResource resource)
    {
        try
        {
            var command = new CreateCollectionCommand(resource.Name, resource.FkIdUser);
            var collection = await commandService.Handle(command);
            if (collection == null) return BadRequest("Could not create collection");
            var collectionResource = CollectionResourceFromEntityAssembler.ToResourceFromEntity(collection);
            return CreatedAtAction(nameof(GetCollectionsByUser), new { userId = collection.FkIdUser }, collectionResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [SwaggerOperation(Summary = "Get collections by user", OperationId = "GetCollectionsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Collections found", typeof(IEnumerable<CollectionResource>))]
    public async Task<IActionResult> GetCollectionsByUser(int userId)
    {
        var query = new GetCollectionsByUserIdQuery(userId);
        var collections = await queryService.Handle(query);
        var resources = collections.Select(CollectionResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Rename a collection", OperationId = "UpdateCollection")]
    [SwaggerResponse(StatusCodes.Status200OK, "Collection updated", typeof(CollectionResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found")]
    public async Task<IActionResult> UpdateCollection(int id, [FromBody] UpdateCollectionResource resource)
    {
        var command = new UpdateCollectionCommand(id, resource.Name);
        var collection = await commandService.Handle(command);
        if (collection == null) return NotFound();
        var collectionResource = CollectionResourceFromEntityAssembler.ToResourceFromEntity(collection);
        return Ok(collectionResource);
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a collection", OperationId = "DeleteCollection")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Collection deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Collection not found")]
    public async Task<IActionResult> DeleteCollection(int id)
    {
        var command = new DeleteCollectionCommand(id);
        var result = await commandService.Handle(command);
        if (result == null) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/routes/{routeId}")]
    [SwaggerOperation(Summary = "Add route to collection", OperationId = "AddRouteToCollection")]
    [SwaggerResponse(StatusCodes.Status201Created, "Route added", typeof(CollectionItemResource))]
    public async Task<IActionResult> AddRouteToCollection(int id, int routeId)
    {
        try
        {
            var command = new AddRouteToCollectionCommand(id, routeId);
            var item = await commandService.Handle(command);
            if (item == null) return BadRequest("Could not add route");
            return Created("", new CollectionItemResource(item.Id, item.FkIdCollection, item.FkIdRoute, item.AddedAt));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/routes/{routeId}")]
    [SwaggerOperation(Summary = "Remove route from collection", OperationId = "RemoveRouteFromCollection")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Route removed")]
    public async Task<IActionResult> RemoveRouteFromCollection(int id, int routeId)
    {
        try
        {
            var command = new RemoveRouteFromCollectionCommand(id, routeId);
            await commandService.Handle(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id}/routes")]
    [SwaggerOperation(Summary = "Get routes in a collection", OperationId = "GetCollectionRoutes")]
    [SwaggerResponse(StatusCodes.Status200OK, "Routes found", typeof(IEnumerable<CollectionItemResource>))]
    public async Task<IActionResult> GetCollectionRoutes(int id)
    {
        var query = new GetCollectionRoutesQuery(id);
        var items = await queryService.Handle(query);
        var resources = items.Select(i => new CollectionItemResource(i.Id, i.FkIdCollection, i.FkIdRoute, i.AddedAt));
        return Ok(resources);
    }
}
