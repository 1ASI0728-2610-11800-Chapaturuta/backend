using System.Net.Mime;
using Frock_backend.IAM.Domain.Model.Commands;
using Frock_backend.IAM.Domain.Model.Queries;
using Frock_backend.IAM.Domain.Services;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.IAM.Interfaces.REST.Resources;
using Frock_backend.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User endpoints")]
public class UsersController(IUserQueryService userQueryService, IUserCommandService userCommandService) : ControllerBase
{
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get a user by its id", OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    public async Task<IActionResult> GetUserById(int id)
    {
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var user = await userQueryService.Handle(getUserByIdQuery);
        if (user == null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all users", OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "The users were found", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers()
    {
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery);
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }

    [HttpGet("email/{email}")]
    [SwaggerOperation(Summary = "Get a user by its email", OperationId = "GetUserByEmail")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var getUserByEmailQuery = new GetUserByEmailQuery(email);
        var user = await userQueryService.Handle(getUserByEmailQuery);
        if (user == null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update user profile", OperationId = "UpdateUserProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was updated", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserProfileResource resource)
    {
        var command = new UpdateUserProfileCommand(id, resource.Username, resource.Email);
        var user = await userCommandService.Handle(command);
        if (user == null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    [HttpPut("{id}/role")]
    [SwaggerOperation(Summary = "Update user role (admin only)", OperationId = "UpdateUserRole")]
    [SwaggerResponse(StatusCodes.Status200OK, "The role was updated", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleResource resource)
    {
        var command = new UpdateUserRoleCommand(id, resource.Role);
        var user = await userCommandService.Handle(command);
        if (user == null) return NotFound();
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
        return Ok(userResource);
    }

    [HttpPost("driver-profile")]
    [SwaggerOperation(Summary = "Create driver profile", OperationId = "CreateDriverProfile")]
    [SwaggerResponse(StatusCodes.Status201Created, "Driver profile created", typeof(DriverProfileResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Profile already exists")]
    public async Task<IActionResult> CreateDriverProfile([FromBody] CreateDriverProfileResource resource)
    {
        try
        {
            var command = new CreateDriverProfileCommand(
                resource.FkIdUser,
                resource.LicenseNumber,
                resource.VehiclePlate,
                resource.VehicleModel,
                resource.VehicleYear,
                resource.VehicleCapacity);
            var profile = await userCommandService.Handle(command);
            if (profile == null) return BadRequest("Could not create driver profile");
            var profileResource = DriverProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
            return CreatedAtAction(nameof(GetDriverProfile), new { userId = profile.FkIdUser }, profileResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("driver-profile/{userId}")]
    [SwaggerOperation(Summary = "Get driver profile by user ID", OperationId = "GetDriverProfile")]
    [SwaggerResponse(StatusCodes.Status200OK, "Driver profile found", typeof(DriverProfileResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Driver profile not found")]
    public async Task<IActionResult> GetDriverProfile(int userId)
    {
        var query = new GetDriverProfileByUserIdQuery(userId);
        var profile = await userQueryService.Handle(query);
        if (profile == null) return NotFound();
        var resource = DriverProfileResourceFromEntityAssembler.ToResourceFromEntity(profile);
        return Ok(resource);
    }
}