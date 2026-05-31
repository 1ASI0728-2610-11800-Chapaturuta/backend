using System.Globalization;
using Frock_backend.Payments.Domain.Model.Commands;
using Frock_backend.Payments.Domain.Repositories;
using Frock_backend.Payments.Domain.Services;
using Frock_backend.Payments.Domain.Services.Gateways;
using Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;
using Frock_backend.Payments.Interfaces.REST.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Frock_backend.Payments.Interfaces.REST;

[ApiController]
[Route("api/v1/payments/payu")]
public class PayUController(
    IPayUPaymentGateway gateway,
    IPaymentRepository paymentRepository,
    IPaymentCommandService paymentCommandService,
    IOptions<PayUSettings> settings) : ControllerBase
{
    private readonly PayUSettings _settings = settings.Value;

    [HttpPost("{paymentId:int}/charge")]
    [SwaggerOperation(Summary = "Carga una tarjeta tokenizada vía PayU para el pago indicado")]
    public async Task<IActionResult> Charge(int paymentId, [FromBody] PayUChargeResource resource)
    {
        var payment = await paymentRepository.FindByIdAsync(paymentId);
        if (payment == null) return NotFound();

        var input = new PayUChargeInput(
            resource.CardToken,
            resource.PayerFullName,
            resource.PayerEmail,
            resource.PayerDocumentNumber,
            resource.PaymentMethodBrand,
            resource.PayerIpAddress,
            resource.DeviceSessionId,
            resource.PayerUserAgent,
            resource.PayerCookie);

        var result = await gateway.ChargeWithTokenAsync(payment, input);
        if (!result.Success)
            return BadRequest(new { result.Message });

        if (!string.IsNullOrEmpty(result.ExternalReference))
            await paymentCommandService.Handle(new ConfirmPaymentCommand(paymentId, result.ExternalReference));

        return Accepted(new { paymentId, externalReference = result.ExternalReference, message = result.Message });
    }

    // PayU confirmation webhook. Configure this URL in the PayU admin panel as "Confirmation URL".
    // Form-encoded POST body. Validates MD5 signature, then dispatches Confirm/Fail.
    [HttpPost("webhook")]
    [Consumes("application/x-www-form-urlencoded")]
    [SwaggerOperation(Summary = "Webhook de confirmación de PayU (no llamar manualmente)")]
    public async Task<IActionResult> Webhook([FromForm] IFormCollection form)
    {
        var sign = form["sign"].ToString();
        var statePol = form["state_pol"].ToString();
        var referenceSale = form["reference_sale"].ToString();
        var amountFormatted = form["value"].ToString();
        var currency = form["currency"].ToString();
        var transactionId = form["transaction_id"].ToString();

        var expected = PayUSignature.ForWebhook(_settings.ApiKey, _settings.MerchantId, referenceSale, amountFormatted, currency, statePol);
        if (!string.Equals(expected, sign, StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { error = "Invalid PayU signature" });

        if (!TryExtractPaymentId(referenceSale, out var paymentId))
            return BadRequest(new { error = "Unknown reference_sale" });

        switch (statePol)
        {
            case "4":
                await paymentCommandService.Handle(new ConfirmPaymentCommand(paymentId, transactionId));
                break;
            case "6":
            case "5":
                await paymentCommandService.Handle(new FailPaymentCommand(paymentId));
                break;
        }

        return Ok();
    }

    private static bool TryExtractPaymentId(string referenceSale, out int paymentId)
    {
        paymentId = 0;
        if (string.IsNullOrEmpty(referenceSale)) return false;
        var parts = referenceSale.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[^1], NumberStyles.Integer, CultureInfo.InvariantCulture, out paymentId);
    }
}
