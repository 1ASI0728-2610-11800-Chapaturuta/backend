using Frock_backend.IAM.Domain.Model.ValueObjects;
using Frock_backend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using Frock_backend.Subscriptions.Domain.Model.Commands;
using Frock_backend.Subscriptions.Domain.Model.Queries;
using Frock_backend.Subscriptions.Domain.Model.ValueObjects;
using Frock_backend.Subscriptions.Domain.Services;
using Frock_backend.Subscriptions.Interfaces.REST.Resources;
using Frock_backend.Subscriptions.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Subscriptions.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/plans")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Plans")]
public class PlansController(
    IPlanCommandService planCommandService,
    IPlanQueryService planQueryService) : ControllerBase
{
    [HttpPost]
    [Authorize(Role.Admin)]
    [SwaggerOperation(
        Summary = "Crear un nuevo plan de suscripcion",
        Description = "Crea un plan de suscripcion (Free o Premium) definiendo precio, ciclo de facturacion, beneficios y cuota Discovery. Solo administradores.",
        OperationId = "CreatePlan")]
    [SwaggerResponse(StatusCodes.Status201Created, "Plan creado correctamente", typeof(PlanResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Datos invalidos para crear el plan")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "No autenticado")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Rol insuficiente: se requiere Admin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanResource resource)
    {
        try
        {
            var command = CreatePlanCommandFromResourceAssembler.ToCommandFromResource(resource);
            var plan = await planCommandService.Handle(command);
            if (plan == null) return BadRequest("Could not create plan");
            var planResource = PlanResourceFromEntityAssembler.ToResourceFromEntity(plan);
            return CreatedAtAction(nameof(GetPlanById), new { id = plan.Id }, planResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}")]
    [Authorize(Role.Admin)]
    [SwaggerOperation(
        Summary = "Actualizar un plan existente",
        Description = "Permite a un administrador actualizar precio, beneficios, cuota Discovery y estado activo de un plan.",
        OperationId = "UpdatePlan")]
    [SwaggerResponse(StatusCodes.Status200OK, "Plan actualizado correctamente", typeof(PlanResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Plan no encontrado")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "No autenticado")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Rol insuficiente: se requiere Admin")]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanResource resource)
    {
        try
        {
            var command = new UpdatePlanCommand(id, resource.Price, resource.Benefits ?? string.Empty, resource.DiscoveryQuota, resource.IsActive);
            var plan = await planCommandService.Handle(command);
            if (plan == null) return NotFound();
            return Ok(PlanResourceFromEntityAssembler.ToResourceFromEntity(plan));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar todos los planes",
        Description = "Devuelve todos los planes de suscripcion registrados, activos e inactivos.",
        OperationId = "GetAllPlans")]
    [SwaggerResponse(StatusCodes.Status200OK, "Listado de planes", typeof(IEnumerable<PlanResource>))]
    public async Task<IActionResult> GetAllPlans()
    {
        var plans = await planQueryService.Handle(new GetAllPlansQuery());
        var resources = plans.Select(PlanResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Obtener un plan por su identificador",
        Description = "Devuelve el detalle de un plan especifico segun su ID.",
        OperationId = "GetPlanById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Plan encontrado", typeof(PlanResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Plan no encontrado")]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var plan = await planQueryService.Handle(new GetPlanByIdQuery(id));
        if (plan == null) return NotFound();
        return Ok(PlanResourceFromEntityAssembler.ToResourceFromEntity(plan));
    }

    [HttpGet("by-target-role/{role}")]
    [SwaggerOperation(
        Summary = "Listar planes activos por rol objetivo",
        Description = "Devuelve los planes activos disponibles para un rol (Traveller, Driver o Both).",
        OperationId = "GetActivePlansByTargetRole")]
    [SwaggerResponse(StatusCodes.Status200OK, "Planes activos para el rol indicado", typeof(IEnumerable<PlanResource>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Rol invalido")]
    public async Task<IActionResult> GetActivePlansByTargetRole(string role)
    {
        if (!Enum.TryParse<TargetRole>(role, true, out var parsedRole))
            return BadRequest(new { message = $"Invalid target role: {role}" });

        var plans = await planQueryService.Handle(new GetActivePlansByTargetRoleQuery(parsedRole));
        var resources = plans.Select(PlanResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
