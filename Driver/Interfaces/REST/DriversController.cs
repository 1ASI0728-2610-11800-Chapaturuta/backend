using Frock_backend.Driver.Domain.Model.Commands;
using Frock_backend.Driver.Domain.Model.Queries;
using Frock_backend.Driver.Domain.Model.ValueObjects;
using Frock_backend.Driver.Domain.Services;
using Frock_backend.Driver.Interfaces.REST.Resources;
using Frock_backend.Driver.Interfaces.REST.Transform;
using Frock_backend.shared.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Driver.Interfaces.REST;

[ApiController]
[Route("api/v1/drivers")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Drivers")]
public class DriversController(
    IDriverCommandService commandService,
    IDriverQueryService queryService,
    ICloudinaryService cloudinaryService) : ControllerBase
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    [HttpPost]
    [SwaggerOperation(Summary = "Create a driver", OperationId = "CreateDriver")]
    [SwaggerResponse(StatusCodes.Status201Created, "Driver created", typeof(DriverResource))]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverResource resource)
    {
        try
        {
            var command = CreateDriverCommandFromResourceAssembler.ToCommandFromResource(resource);
            var driver = await commandService.Handle(command);
            if (driver == null) return BadRequest("Could not create driver");
            var driverResource = DriverResourceFromEntityAssembler.ToResourceFromEntity(driver);
            return CreatedAtAction(nameof(GetDriverById), new { id = driver.Id }, driverResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all drivers", OperationId = "GetAllDrivers")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drivers found", typeof(IEnumerable<DriverResource>))]
    public async Task<IActionResult> GetAllDrivers()
    {
        var drivers = await queryService.Handle(new GetAllDriversQuery());
        var resources = drivers.Select(DriverResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a driver by id", OperationId = "GetDriverById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Driver found", typeof(DriverResource))]
    public async Task<IActionResult> GetDriverById(int id)
    {
        var driver = await queryService.Handle(new GetDriverByIdQuery(id));
        if (driver == null) return NotFound();
        return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
    }

    [HttpGet("by-user/{userId:int}")]
    [SwaggerOperation(Summary = "Get a driver by IAM user id", OperationId = "GetDriverByUserId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Driver found", typeof(DriverResource))]
    public async Task<IActionResult> GetDriverByUserId(int userId)
    {
        var driver = await queryService.Handle(new GetDriverByFkIdUserQuery(userId));
        if (driver == null) return NotFound();
        return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
    }

    [HttpGet("by-vehicle-type/{vehicleType}")]
    [SwaggerOperation(Summary = "Get drivers filtered by vehicle type", OperationId = "GetDriversByVehicleType")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drivers found", typeof(IEnumerable<DriverResource>))]
    public async Task<IActionResult> GetDriversByVehicleType(VehicleType vehicleType)
    {
        var drivers = await queryService.Handle(new GetDriversByVehicleTypeQuery(vehicleType));
        var resources = drivers.Select(DriverResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("available")]
    [SwaggerOperation(Summary = "Get drivers available on a given day", OperationId = "GetAvailableDriversByDay")]
    [SwaggerResponse(StatusCodes.Status200OK, "Drivers found", typeof(IEnumerable<DriverResource>))]
    public async Task<IActionResult> GetAvailableDriversByDay([FromQuery] DayOfWeek day)
    {
        var drivers = await queryService.Handle(new GetAvailableDriversByDayOfWeekQuery(day));
        var resources = drivers.Select(DriverResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpPatch("{id:int}")]
    [SwaggerOperation(Summary = "Update driver personal info", OperationId = "UpdateDriver")]
    [SwaggerResponse(StatusCodes.Status200OK, "Driver updated", typeof(DriverResource))]
    public async Task<IActionResult> UpdateDriver(int id, [FromBody] UpdateDriverResource resource)
    {
        try
        {
            var command = new UpdateDriverCommand(id, resource.FirstName, resource.LastName, resource.Phone, resource.PhotoUrl);
            var driver = await commandService.Handle(command);
            if (driver == null) return NotFound();
            return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/vehicle")]
    [SwaggerOperation(Summary = "Update driver vehicle", OperationId = "UpdateDriverVehicle")]
    [SwaggerResponse(StatusCodes.Status200OK, "Vehicle updated", typeof(DriverResource))]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleResource resource)
    {
        try
        {
            var command = new UpdateVehicleCommand(
                id,
                resource.Plate,
                resource.Brand,
                resource.Model,
                resource.Year,
                resource.Capacity,
                resource.VehicleType);
            var driver = await commandService.Handle(command);
            if (driver == null) return NotFound();
            return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/availability")]
    [SwaggerOperation(Summary = "Toggle driver availability", OperationId = "ToggleDriverAvailability")]
    [SwaggerResponse(StatusCodes.Status200OK, "Availability toggled", typeof(DriverResource))]
    public async Task<IActionResult> ToggleAvailability(int id)
    {
        try
        {
            var command = new ToggleAvailabilityCommand(id);
            var driver = await commandService.Handle(command);
            if (driver == null) return NotFound();
            return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/photo")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "Upload driver photo to Cloudinary", OperationId = "UploadDriverPhoto")]
    [SwaggerResponse(StatusCodes.Status200OK, "Photo uploaded", typeof(DriverResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Driver not found")]
    public async Task<IActionResult> UploadDriverPhoto(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided" });
            if (file.Length > MaxPhotoBytes)
                return BadRequest(new { message = "File exceeds 5 MB limit" });
            if (!AllowedImageTypes.Contains(file.ContentType))
                return BadRequest(new { message = "Only JPEG, PNG or WebP images are allowed" });

            var url = await cloudinaryService.UploadImageAsync(file, "drivers");
            var driver = await commandService.Handle(new UpdateDriverPhotoCommand(id, url));
            if (driver == null) return NotFound();
            return Ok(DriverResourceFromEntityAssembler.ToResourceFromEntity(driver));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(Summary = "Soft-delete a driver", OperationId = "DeleteDriver")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Driver deleted")]
    public async Task<IActionResult> DeleteDriver(int id)
    {
        try
        {
            var ok = await commandService.Handle(new DeleteDriverCommand(id));
            if (!ok) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
