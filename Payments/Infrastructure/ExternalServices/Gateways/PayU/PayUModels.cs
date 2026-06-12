using System.Text.Json.Serialization;

namespace Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;

public record PayUChargeInput(
    string CardNumber,
    string CardSecurityCode,
    string CardExpirationDate, // YYYY/MM
    string CardHolderName,
    string PayerFullName,
    string PayerEmail,
    string PayerDocumentNumber,
    string PaymentMethodBrand,
    string PayerIpAddress,
    string DeviceSessionId,
    string PayerUserAgent,
    string PayerCookie);

public class PayURequest
{
    [JsonPropertyName("language")] public string Language { get; set; } = "es";
    [JsonPropertyName("command")] public string Command { get; set; } = "SUBMIT_TRANSACTION";
    [JsonPropertyName("merchant")] public PayUMerchant Merchant { get; set; } = new();
    [JsonPropertyName("transaction")] public PayUTransaction Transaction { get; set; } = new();
    [JsonPropertyName("test")] public bool Test { get; set; }
}

public class PayUMerchant
{
    [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = string.Empty;
    [JsonPropertyName("apiLogin")] public string ApiLogin { get; set; } = string.Empty;
}

public class PayUTransaction
{
    [JsonPropertyName("order")] public PayUOrder Order { get; set; } = new();
    [JsonPropertyName("payer")] public PayUPayer? Payer { get; set; }
    [JsonPropertyName("creditCardTokenId")] public string? CreditCardTokenId { get; set; }
    [JsonPropertyName("creditCard")] public PayUCreditCard? CreditCard { get; set; }
    [JsonPropertyName("extraParameters")] public Dictionary<string, object>? ExtraParameters { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = "AUTHORIZATION_AND_CAPTURE";
    [JsonPropertyName("paymentMethod")] public string PaymentMethod { get; set; } = "VISA";
    [JsonPropertyName("paymentCountry")] public string PaymentCountry { get; set; } = "PE";
    [JsonPropertyName("deviceSessionId")] public string? DeviceSessionId { get; set; }
    [JsonPropertyName("ipAddress")] public string? IpAddress { get; set; }
    [JsonPropertyName("cookie")] public string? Cookie { get; set; }
    [JsonPropertyName("userAgent")] public string? UserAgent { get; set; }
    [JsonPropertyName("parentTransactionId")] public string? ParentTransactionId { get; set; }
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}

public class PayUOrder
{
    [JsonPropertyName("id")] public long? Id { get; set; }
    [JsonPropertyName("accountId")] public string AccountId { get; set; } = string.Empty;
    [JsonPropertyName("referenceCode")] public string ReferenceCode { get; set; } = string.Empty;
    [JsonPropertyName("description")] public string Description { get; set; } = string.Empty;
    [JsonPropertyName("language")] public string Language { get; set; } = "es";
    [JsonPropertyName("signature")] public string Signature { get; set; } = string.Empty;
    [JsonPropertyName("notifyUrl")] public string? NotifyUrl { get; set; }
    [JsonPropertyName("additionalValues")] public PayUAdditionalValues AdditionalValues { get; set; } = new();
    [JsonPropertyName("buyer")] public PayUBuyer? Buyer { get; set; }
}

public class PayUAdditionalValues
{
    [JsonPropertyName("TX_VALUE")] public PayUMoney TxValue { get; set; } = new();
}

public class PayUMoney
{
    [JsonPropertyName("value")] public decimal Value { get; set; }
    [JsonPropertyName("currency")] public string Currency { get; set; } = "PEN";
}

public class PayUBuyer
{
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("emailAddress")] public string EmailAddress { get; set; } = string.Empty;
    [JsonPropertyName("dniNumber")] public string? DniNumber { get; set; }
}

public class PayUCreditCard
{
    [JsonPropertyName("number")] public string Number { get; set; } = string.Empty;
    [JsonPropertyName("securityCode")] public string SecurityCode { get; set; } = string.Empty;
    [JsonPropertyName("expirationDate")] public string ExpirationDate { get; set; } = string.Empty; // YYYY/MM
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public class PayUPayer
{
    [JsonPropertyName("fullName")] public string FullName { get; set; } = string.Empty;
    [JsonPropertyName("emailAddress")] public string EmailAddress { get; set; } = string.Empty;
    [JsonPropertyName("dniNumber")] public string? DniNumber { get; set; }
    [JsonPropertyName("contactPhone")] public string? ContactPhone { get; set; }
}

public class PayUResponse
{
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("error")] public string? Error { get; set; }
    [JsonPropertyName("transactionResponse")] public PayUTransactionResponse? TransactionResponse { get; set; }
}

public class PayUTransactionResponse
{
    [JsonPropertyName("orderId")] public long? OrderId { get; set; }
    [JsonPropertyName("transactionId")] public string? TransactionId { get; set; }
    [JsonPropertyName("state")] public string? State { get; set; }
    [JsonPropertyName("responseCode")] public string? ResponseCode { get; set; }
    [JsonPropertyName("responseMessage")] public string? ResponseMessage { get; set; }
    [JsonPropertyName("authorizationCode")] public string? AuthorizationCode { get; set; }
}
