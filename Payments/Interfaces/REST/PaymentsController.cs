using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Model.Queries;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Payments.Interfaces.REST.Resources;
using Frock_backend.Payments.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace Frock_backend.Payments.Interfaces.REST;

[ApiController]
[Route("api/v1/payments")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Payments")]
public class PaymentsController(
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService,
    IRefundCommandService refundCommandService,
    IRefundQueryService refundQueryService) : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(Summary = "Create a payment", OperationId = "CreatePayment")]
    [SwaggerResponse(StatusCodes.Status201Created, "Payment created", typeof(PaymentResource))]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentResource resource)
    {
        try
        {
            var command = CreatePaymentCommandFromResourceAssembler.ToCommandFromResource(resource);
            var payment = await paymentCommandService.Handle(command);
            if (payment == null) return BadRequest("Could not create payment");
            var paymentResource = PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment);
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, paymentResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/confirm")]
    [SwaggerOperation(Summary = "Confirm a payment", OperationId = "ConfirmPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment confirmed", typeof(PaymentResource))]
    public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentResource resource)
    {
        try
        {
            var command = new ConfirmPaymentCommand(id, resource.ExternalReference);
            var payment = await paymentCommandService.Handle(command);
            if (payment == null) return NotFound();
            return Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/fail")]
    [SwaggerOperation(Summary = "Mark a payment as failed", OperationId = "FailPayment")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment marked as failed", typeof(PaymentResource))]
    public async Task<IActionResult> FailPayment(int id)
    {
        try
        {
            var command = new FailPaymentCommand(id);
            var payment = await paymentCommandService.Handle(command);
            if (payment == null) return NotFound();
            return Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:int}/refunds")]
    [SwaggerOperation(Summary = "Create a refund for a payment", OperationId = "CreateRefund")]
    [SwaggerResponse(StatusCodes.Status201Created, "Refund created", typeof(RefundResource))]
    public async Task<IActionResult> CreateRefund(int id, [FromBody] CreateRefundResource resource)
    {
        try
        {
            var command = CreateRefundCommandFromResourceAssembler.ToCommandFromResource(id, resource);
            var refund = await refundCommandService.Handle(command);
            if (refund == null) return BadRequest("Could not create refund");
            var refundResource = RefundResourceFromEntityAssembler.ToResourceFromEntity(refund);
            return CreatedAtAction(nameof(GetRefundsByPaymentId), new { id = id }, refundResource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("/api/v1/refunds/{id:int}/confirm")]
    [SwaggerOperation(Summary = "Confirm a refund", OperationId = "ConfirmRefund")]
    [SwaggerResponse(StatusCodes.Status200OK, "Refund confirmed", typeof(RefundResource))]
    public async Task<IActionResult> ConfirmRefund(int id)
    {
        try
        {
            var command = new ConfirmRefundCommand(id);
            var refund = await refundCommandService.Handle(command);
            if (refund == null) return NotFound();
            return Ok(RefundResourceFromEntityAssembler.ToResourceFromEntity(refund));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(Summary = "Get a payment by id", OperationId = "GetPaymentById")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payment found", typeof(PaymentResource))]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await paymentQueryService.Handle(new GetPaymentByIdQuery(id));
        if (payment == null) return NotFound();
        return Ok(PaymentResourceFromEntityAssembler.ToResourceFromEntity(payment));
    }

    [HttpGet("user/{userId:int}")]
    [SwaggerOperation(Summary = "Get payments by user", OperationId = "GetPaymentsByUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "Payments found", typeof(IEnumerable<PaymentResource>))]
    public async Task<IActionResult> GetPaymentsByUser(int userId)
    {
        var payments = await paymentQueryService.Handle(new GetPaymentsByUserIdQuery(userId));
        var resources = payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet("{id:int}/refunds")]
    [SwaggerOperation(Summary = "Get refunds for a payment", OperationId = "GetRefundsByPaymentId")]
    [SwaggerResponse(StatusCodes.Status200OK, "Refunds found", typeof(IEnumerable<RefundResource>))]
    public async Task<IActionResult> GetRefundsByPaymentId(int id)
    {
        var refunds = await refundQueryService.Handle(new GetRefundsByPaymentIdQuery(id));
        var resources = refunds.Select(RefundResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
}
