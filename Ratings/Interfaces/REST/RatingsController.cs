using Frock_backend.Ratings.Domain.Model.Commands;
using Frock_backend.Ratings.Domain.Model.Queries;
using Frock_backend.Ratings.Domain.Services;
using Frock_backend.Ratings.Interfaces.REST.Resources;
using Frock_backend.Ratings.Interfaces.REST.Transform;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.IAM.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Ratings.Interfaces.REST;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Ratings")]
public class RatingsController(IRatingCommandService commandService, IRatingQueryService queryService) : ControllerBase
{
    [HttpPost]
    [Authorize(Role.Traveller, Role.Admin)]
    [SwaggerOperation(Summary = "Create a rating", OperationId = "CreateRating")]
    [SwaggerResponse(StatusCodes.Status201Created, "Rating created", typeof(RatingResource))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized - token missing or invalid")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden - insufficient role")]
    public async Task<IActionResult> CreateRating([FromBody] CreateRatingResource resource)
    {
        try
        {
            var command = new CreateRatingCommand(resource.FkIdUser, resource.FkIdDriver, resource.FkIdTrip, resource.Score, resource.Comment);
            var rating = await commandService.Handle(command);
            if (rating == null) return BadRequest("Could not create rating");
            var ratingResource = RatingResourceFromEntityAssembler.ToResourceFromEntity(rating);
            return CreatedAtAction(nameof(GetRatingsByDriver), new { driverId = rating.FkIdDriver }, ratingResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("driver/{driverId}")]
    [SwaggerOperation(Summary = "Get ratings for a driver", OperationId = "GetRatingsByDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ratings found", typeof(IEnumerable<RatingResource>))]
    public async Task<IActionResult> GetRatingsByDriver(int driverId)
    {
        var query = new GetRatingsByDriverIdQuery(driverId);
        var ratings = await queryService.Handle(query);
        var resources = ratings.Select(RatingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("driver/{driverId}/summary")]
    [SwaggerOperation(Summary = "Get driver rating summary", OperationId = "GetDriverRatingSummary")]
    [SwaggerResponse(StatusCodes.Status200OK, "Summary found", typeof(RatingSummaryResource))]
    public async Task<IActionResult> GetDriverRatingSummary(int driverId)
    {
        var query = new GetDriverRatingSummaryQuery(driverId);
        var (average, count) = await queryService.Handle(query);
        return Ok(new RatingSummaryResource(driverId, Math.Round(average, 2), count));
    }

    [HttpGet("user/{userId}")]
    [SwaggerOperation(Summary = "Get ratings by user", OperationId = "GetRatingsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Ratings found", typeof(IEnumerable<RatingResource>))]
    public async Task<IActionResult> GetRatingsByUser(int userId)
    {
        var query = new GetRatingsByUserIdQuery(userId);
        var ratings = await queryService.Handle(query);
        var resources = ratings.Select(RatingResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
