using Frock_backend.stops.Domain.Model.Commands;
using Frock_backend.stops.Domain.Model.Queries;
using Frock_backend.stops.Domain.Services;
using Frock_backend.stops.Interfaces.REST.Resources;
using Frock_backend.stops.Interfaces.REST.Transform;
using Frock_backend.shared.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.stops.Interfaces.REST
{
    /// <summary>
    /// Stops controller.
    /// </summary>
    /// <param name="stopCommandService">The Stop Command Service</param>
    /// <param name="stopCommandServiceQueryService">The Stop Query Service</param>
    /// <param name="cloudinaryService">The Cloudinary Service</param>
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Stops")]
    public class StopsController(
        IStopCommandService stopCommandService,
        IStopQueryService stopQueryService,
        ICloudinaryService cloudinaryService) : ControllerBase
    {
        /// <summary>
        /// Creates a new stop with optional image upload.
        /// </summary>
        /// <param name="resource">The CreateStopFormResource resource</param>
        /// <returns>
        /// A response as an action result containing the created stop, or bad request if the stop was not created.
        /// </returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Creates a new stop with optional image upload.",
            Description = "Creates a new stop with given parameters and optionally uploads image to Cloudinary",
            OperationId = "CreateStop"
            )]
        [SwaggerResponse(201, "The stop was created", typeof(StopResource))]
        [SwaggerResponse(400, "The stop was not created")]
        [SwaggerResponse(500, "An error occurred while creating the stop")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(StopResource))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult> CreateStop([FromForm] CreateStopFormResource resource)
        {
            try
            {
                string imageUrl = string.Empty;

                // Subir imagen a Cloudinary si se proporciona
                if (resource.ImageFile != null && resource.ImageFile.Length > 0)
                {
                    imageUrl = await cloudinaryService.UploadImageAsync(resource.ImageFile, "stops");
                }

                // Crear el resource tradicional con la URL de la imagen
                var createStopResource = new CreateStopResource(
                    resource.Name,
                    resource.GoogleMapsUrl ?? string.Empty,
                    imageUrl,
                    resource.Phone,
                    resource.FkIdCompany,
                    resource.Address,
                    resource.Reference,
                    resource.FkIdDistrict,
                    resource.Latitude,
                    resource.Longitude
                );

                var createStopCommand = CreateStopCommandFromResourceAssembler.ToCommandFromResource(createStopResource);
                var result = await stopCommandService.Handle(createStopCommand);

                if (result is null) return BadRequest("Could not create stop");

                return CreatedAtAction(nameof(GetStopById), new { id = result.Id }, StopResourceFromEntityAssembler.ToResourceFromEntity(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets a stop by ID.
        /// </summary>
        /// <param name="id">The ID of the stop</param>
        /// <returns>
        /// A response as an action result containing the stop, or not found if the stop was not found.
        /// </returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
               Summary = "Gets a stop by id",
               Description = "Gets a stop for a given stop identifier",
               OperationId = "GetStopById")]
        [SwaggerResponse(200, "The stop was found", typeof(StopResource))]
        public async Task<ActionResult> GetStopById(int id)
        {
            var getStopByIdQuery = new GetStopByIdQuery(id);
            var result = await stopQueryService.Handle(getStopByIdQuery);
            if (result is null) return NotFound();
            var resource = StopResourceFromEntityAssembler.ToResourceFromEntity(result);
            return Ok(resource);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Gets all stops",
            Description = "Gets all stops in the system",
            OperationId = "GetAllStops")]
        [SwaggerResponse(200, "The stops were found", typeof(IEnumerable<StopResource>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No stops found")]

        public async Task<IActionResult> GetAllStops()
        {
            var getAllStopsQuery = new GetAllStopsQuery();
            var result = await stopQueryService.Handle(getAllStopsQuery);
            if (result == null || !result.Any())
            {
                return NotFound();
            }
            var resources = result.Select(StopResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }

        [HttpGet("company/{FkIdCompany}")]
        [SwaggerOperation(
       Summary = "Gets all stops by FkIdCompany",
       Description = "Gets a stop for a given company identifier",
       OperationId = "GetStopsByFkIdCompany")]
        [SwaggerResponse(200, "The stops were found", typeof(IEnumerable<StopResource>))]
        public async Task<ActionResult> GetStopsByFkIdCompany(int FkIdCompany)
        {
            var getAllStopsByFkIdCompanyQuery = new GetAllStopsByFkIdCompanyQuery(FkIdCompany);
            var result = await stopQueryService.Handle(getAllStopsByFkIdCompanyQuery);

            if (result == null)
            {
                return NotFound();
            }

            var resources = result.Select(StopResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }

        [HttpGet("District/{FkIdDistrict}")]
        [SwaggerOperation(
       Summary = "Gets all stops by FkIdDistrict",
       Description = "Gets a stop for a given District identifier",
       OperationId = "GetStopsByFkIdDistrict")]

        [SwaggerResponse(200, "The stops were found", typeof(IEnumerable<StopResource>))] // Updated return type
        [SwaggerResponse(StatusCodes.Status200OK, "The stops were found", typeof(IEnumerable<StopResource>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No stops found for the District or District not found")] // Added 404 response

        public async Task<ActionResult> GetStopsByFkIdDistrict(int FkIdDistrict)
        {
            var getAllStopsByFkIdDistrictQuery = new GetAllStopsByFkIdDistrictQuery(FkIdDistrict);
            var result = await stopQueryService.Handle(getAllStopsByFkIdDistrictQuery);

            if (result == null)
            {
                return NotFound();
            }

            var resources = result.Select(StopResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }

        [HttpGet("district/{FkIdDistrict}/name/{Name}")]
        [SwaggerOperation(
            Summary = "Gets a stop by District ID and name",
            Description = "Gets a specific stop for a given District ID and stop name",
            OperationId = "GetStopByDistrictAndName")]
        [SwaggerResponse(StatusCodes.Status200OK, "The stop was found", typeof(StopResource))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The stop was not found for the given District and name")]
        public async Task<ActionResult> GetStopByNameAndFkIdDistrict(int FkIdDistrict, string Name)
        {
            var getStopByNameAndDistrictQuery = new GetStopByNameAndFkIdDistrictQuery(Name, FkIdDistrict);
            var result = await stopQueryService.Handle(getStopByNameAndDistrictQuery);

            if (result is null)
            {
                return NotFound();
            }

            var resource = StopResourceFromEntityAssembler.ToResourceFromEntity(result);
            return Ok(resource);
        }

        //get by name and company
        [HttpGet("company/{FkIdCompany}/name/{Name}")]
        [SwaggerOperation(
            Summary = "Gets a stop by Company ID and name",
            Description = "Gets a specific stop for a given Company ID and stop name",
            OperationId = "GetStopByNameAndFkIdCompany")]
        [SwaggerResponse(StatusCodes.Status200OK, "The stop was found", typeof(StopResource))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The stop was not found for the given Company and name")]
        public async Task<ActionResult> GetStopByNameAndFkIdCompany(int FkIdCompany, string Name)
        {
            var getStopByNameAndCompanyQuery = new GetStopByNameAndFkIdCompanyQuery(Name, FkIdCompany);
            var result = await stopQueryService.Handle(getStopByNameAndCompanyQuery);
            if (result is null)
            {
                return NotFound();
            }
            var resource = StopResourceFromEntityAssembler.ToResourceFromEntity(result);
            return Ok(resource);
        }



        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Deletes a stop by ID",
            Description = "Deletes a stop for a given stop identifier",
            OperationId = "DeleteStop")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The stop was successfully deleted")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The stop was not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request (e.g., malformed ID)")]
        public async Task<IActionResult> DeleteStop(int id)
        {
            var deleteStopCommand = new DeleteStopCommand(id);
            var deletedStop = await stopCommandService.Handle(deleteStopCommand);

            if (deletedStop is null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Updates an existing stop by ID.",
            Description = "Updates an existing stop with the provided data. The ID in the URL must match the ID in the request body.",
            OperationId = "UpdateStop")]
        [SwaggerResponse(StatusCodes.Status200OK, "The stop was successfully updated.", typeof(StopResource))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data (e.g., ID mismatch or validation error).")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The stop with the specified ID was not found.")]
        public async Task<IActionResult> UpdateStop(int id, [FromBody] UpdateStopResource resource)
        {
            if (id != resource.Id)
            {
                return BadRequest("ID in URL must match ID in request body.");
            }

            var updateStopCommand = UpdateStopCommandFromResourceAssembler.ToCommandFromResource(resource);
            var updatedStop = await stopCommandService.Handle(updateStopCommand);

            if (updatedStop is null)
            {
                var existingStop = await stopQueryService.Handle(new GetStopByIdQuery(id));
                if (existingStop == null)
                {
                    return NotFound($"Stop with ID {id} not found.");
                }
                return BadRequest("Could not update the stop with the provided parameters.");
            }

            var stopResource = StopResourceFromEntityAssembler.ToResourceFromEntity(updatedStop);
            return Ok(stopResource);
        }
    }
}
