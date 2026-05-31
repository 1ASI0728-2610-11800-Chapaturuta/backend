using System.Globalization;
using System.Net.Http.Json;
using Frock_backend.Payments.Domain.Model.Aggregates;
using Frock_backend.Payments.Domain.Services.Gateways;
using Microsoft.Extensions.Options;

namespace Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;

public class PayUPaymentGateway(
    HttpClient httpClient,
    IOptions<PayUSettings> options,
    ILogger<PayUPaymentGateway> logger) : IPayUPaymentGateway
{
    private readonly PayUSettings _settings = options.Value;

    public Task<GatewayResult> InitiateAsync(Payment payment) =>
        Task.FromResult(new GatewayResult(
            Success: false,
            ExternalReference: null,
            Message: "PayU requires a tokenized card. Call POST /api/v1/payments/{id}/payu/charge with PayUChargeInput."));

    public async Task<GatewayResult> ChargeWithTokenAsync(Payment payment, PayUChargeInput input)
    {
        var referenceCode = ReferenceCodeFor(payment);
        var amount = payment.Amount.Amount;
        var currency = payment.Amount.Currency;

        var request = new PayURequest
        {
            Command = "SUBMIT_TRANSACTION",
            Test = _settings.TestMode,
            Merchant = new PayUMerchant { ApiKey = _settings.ApiKey, ApiLogin = _settings.ApiLogin },
            Transaction = new PayUTransaction
            {
                Type = "AUTHORIZATION_AND_CAPTURE",
                PaymentMethod = input.PaymentMethodBrand,
                PaymentCountry = "PE",
                CreditCardTokenId = input.CardToken,
                DeviceSessionId = input.DeviceSessionId,
                IpAddress = input.PayerIpAddress,
                UserAgent = input.PayerUserAgent,
                Cookie = input.PayerCookie,
                ExtraParameters = new Dictionary<string, object> { ["INSTALLMENTS_NUMBER"] = 1 },
                Payer = new PayUPayer
                {
                    FullName = input.PayerFullName,
                    EmailAddress = input.PayerEmail,
                    DniNumber = input.PayerDocumentNumber
                },
                Order = new PayUOrder
                {
                    AccountId = _settings.AccountId,
                    ReferenceCode = referenceCode,
                    Description = $"Frock payment {payment.Id} ({payment.ReferenceType} {payment.ReferenceId})",
                    Language = "es",
                    NotifyUrl = _settings.NotifyUrl,
                    Signature = PayUSignature.ForRequest(_settings.ApiKey, _settings.MerchantId, referenceCode, amount, currency),
                    AdditionalValues = new PayUAdditionalValues { TxValue = new PayUMoney { Value = amount, Currency = currency } },
                    Buyer = new PayUBuyer { FullName = input.PayerFullName, EmailAddress = input.PayerEmail, DniNumber = input.PayerDocumentNumber }
                }
            }
        };

        return await PostAsync(request, expectedSuccess: tr => tr.State == "APPROVED" || tr.State == "PENDING");
    }

    public async Task<GatewayResult> RefundAsync(Payment payment, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(payment.ExternalReference))
            return new GatewayResult(false, null, "Payment has no PayU transaction id to refund");

        var isPartial = amount < payment.Amount.Amount;

        var request = new PayURequest
        {
            Command = "SUBMIT_TRANSACTION",
            Test = _settings.TestMode,
            Merchant = new PayUMerchant { ApiKey = _settings.ApiKey, ApiLogin = _settings.ApiLogin },
            Transaction = new PayUTransaction
            {
                Type = isPartial ? "PARTIAL_REFUND" : "REFUND",
                ParentTransactionId = payment.ExternalReference,
                Reason = "Refund requested by Frock backend",
                Order = new PayUOrder
                {
                    AccountId = _settings.AccountId,
                    ReferenceCode = ReferenceCodeFor(payment),
                    Language = "es",
                    Signature = PayUSignature.ForRequest(_settings.ApiKey, _settings.MerchantId, ReferenceCodeFor(payment), amount, payment.Amount.Currency),
                    AdditionalValues = new PayUAdditionalValues { TxValue = new PayUMoney { Value = amount, Currency = payment.Amount.Currency } }
                }
            }
        };

        return await PostAsync(request, expectedSuccess: tr => tr.State == "APPROVED" || tr.State == "PENDING");
    }

    public Task<GatewayResult> ConfirmAsync(string externalRef) =>
        Task.FromResult(new GatewayResult(true, externalRef, "Confirmation is delivered asynchronously via webhook"));

    private async Task<GatewayResult> PostAsync(PayURequest request, Func<PayUTransactionResponse, bool> expectedSuccess)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(_settings.ApiUrl, request);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("PayU HTTP {Status} for ref {Ref}", (int)response.StatusCode, request.Transaction.Order.ReferenceCode);
                return new GatewayResult(false, null, $"PayU HTTP {(int)response.StatusCode}");
            }

            var payload = await response.Content.ReadFromJsonAsync<PayUResponse>();
            if (payload is null) return new GatewayResult(false, null, "Empty PayU response");

            if (!string.Equals(payload.Code, "SUCCESS", StringComparison.OrdinalIgnoreCase))
                return new GatewayResult(false, null, payload.Error ?? "PayU command failed");

            var tr = payload.TransactionResponse;
            if (tr is null) return new GatewayResult(false, null, "No transactionResponse in PayU payload");

            var ok = expectedSuccess(tr);
            var externalRef = tr.OrderId?.ToString(CultureInfo.InvariantCulture) ?? tr.TransactionId;
            return new GatewayResult(ok, externalRef, tr.ResponseMessage ?? tr.State);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "PayU HTTP failure");
            return new GatewayResult(false, null, $"PayU transport error: {ex.Message}");
        }
    }

    private static string ReferenceCodeFor(Payment payment) => $"FROCK-PAY-{payment.Id}";
}
