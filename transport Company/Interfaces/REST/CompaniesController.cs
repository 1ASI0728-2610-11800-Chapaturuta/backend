using Frock_backend.transport_Company.Domain.Model.Commands;
using Frock_backend.transport_Company.Domain.Model.Queries;
using Frock_backend.transport_Company.Domain.Services;
using Frock_backend.transport_Company.Interfaces.REST.Resources;
using Frock_backend.transport_Company.Interfaces.REST.Transform;
using Frock_backend.shared.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.transport_Company.Interfaces.REST
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    [Tags("Companies")]
    public class CompaniesController(
        ICompanyCommandService commandService, 
        ICompanyQueryService queryService,
        ICloudinaryService cloudinaryService) : ControllerBase
    {
        /// <summary>
        /// Creates a new company with optional logo upload.
        /// </summary>
        /// <param name="resource">The company data including optional logo file</param>
        /// <returns>The newly created company resource.</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Creates a new company with optional logo upload.",
            Description = "Creates a new company with the given parameters and optionally uploads logo to Cloudinary.",
            OperationId = "CreateCompany")]
        [SwaggerResponse(StatusCodes.Status201Created, "The company was created.", typeof(CompanyResource))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The company could not be created due to invalid data.")]
        public async Task<IActionResult> CreateCompany([FromForm] CreateCompanyFormResource resource)
        {
            try
            {
                string logoUrl = string.Empty;

                // Subir imagen a Cloudinary si se proporciona
                if (resource.LogoFile != null && resource.LogoFile.Length > 0)
                {
                    logoUrl = await cloudinaryService.UploadImageAsync(resource.LogoFile, "companies");
                }

                // Crear el resource usando los parámetros
                var createCompanyResource = new CreateCompanyResource(
                    resource.Name,
                    logoUrl,
                    resource.FkIdUser
                );

                var createCompanyCommand = CreateCompanyCommandFromResourceAssembler.ToCommandFromResource(createCompanyResource);
                var company = await commandService.Handle(createCompanyCommand);

                if (company is null) return BadRequest("Could not create company");

                var companyResource = CompanyResourceFromEntityAssembler.ToResourceFromEntity(company);
                return CreatedAtAction(nameof(GetCompanyById), new { id = company.Id }, companyResource);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets all companies.
        /// </summary>
        /// <returns>A list of all companies.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Gets all companies.",
            Description = "Gets a list of all registered companies.",
            OperationId = "GetAllCompanies")]
        [SwaggerResponse(StatusCodes.Status200OK, "The list of companies.", typeof(IEnumerable<CompanyResource>))]
        public async Task<IActionResult> GetAllCompanies()
        {
            var getAllCompaniesQuery = new GetAllCompaniesQuery();
            var companies = await queryService.Handle(getAllCompaniesQuery);
            var resources = companies.Select(CompanyResourceFromEntityAssembler.ToResourceFromEntity);
            return Ok(resources);
        }
        
        [HttpGet("user/{FKeyIdUser}")]
        [SwaggerOperation(
            Summary = "Verify Company by UserID",
            Description = "Returns the redirect route based on whether or not the user has an associated company.",
            OperationId = "CheckUserCompany")]
        [SwaggerResponse(StatusCodes.Status200OK, "The user has a company.", typeof(object))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The user does not have a company.")]
        public async Task<IActionResult> CheckUserCompany(int FKeyIdUser)
        {
            var getCompanyByUserQuery = new GetCompanyByFkIdUserQuery(FKeyIdUser);
            var company = await queryService.Handle(getCompanyByUserQuery);
            if (company is null) return NotFound();
            var resource = CompanyResourceFromEntityAssembler.ToResourceFromEntity(company);
            return Ok(resource);
        }

        /// <summary>
        /// Gets a company by its ID.
        /// </summary>
        /// <param name="id">The company identifier.</param>
        /// <returns>The company resource.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Gets a company by ID.",
            Description = "Gets a company for a given identifier.",
            OperationId = "GetCompanyById")]
        [SwaggerResponse(StatusCodes.Status200OK, "The company was found.", typeof(CompanyResource))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The company was not found.")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var getCompanyByIdQuery = new GetCompanyByIdQuery(id);
            var company = await queryService.Handle(getCompanyByIdQuery);
            if (company is null) return NotFound();

            var resource = CompanyResourceFromEntityAssembler.ToResourceFromEntity(company);
            return Ok(resource);
        }
        
        /// <summary>
        /// Updates an existing company.
        /// </summary>
        /// <param name="id">The company identifier.</param>
        /// <param name="resource">The updated company data.</param>
        /// <returns>The updated company resource.</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Updates an existing company.",
            Description = "Updates an existing company with the provided data.",
            OperationId = "UpdateCompany")]
        [SwaggerResponse(StatusCodes.Status200OK, "The company was successfully updated.", typeof(CompanyResource))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request data.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The company was not found.")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyResource resource)
        {
            if (id != resource.Id)
                return BadRequest("ID in URL must match ID in request body.");

            var updateCompanyCommand = UpdateCompanyCommandFromResourceAssembler.ToCommandFromResource(resource);
            var updatedCompany = await commandService.Handle(updateCompanyCommand);
            if (updatedCompany is null) return NotFound();

            var companyResource = CompanyResourceFromEntityAssembler.ToResourceFromEntity(updatedCompany);
            return Ok(companyResource);
        }

        /// <summary>
        /// Deletes a company by its ID.
        /// </summary>
        /// <param name="id">The company identifier.</param>
        /// <returns>No content if deletion was successful.</returns>
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Deletes a company by ID.",
            Description = "Deletes a company for a given identifier.",
            OperationId = "DeleteCompany")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The company was successfully deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The company was not found.")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var deleteCompanyCommand = new DeleteCompanyCommand(id);
            var result = await commandService.Handle(deleteCompanyCommand);
            if (result is null) return NotFound();

            return NoContent();
        }
    }
}