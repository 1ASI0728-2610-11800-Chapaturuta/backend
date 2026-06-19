namespace Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;

public class PayUSettings
{
    public string ApiUrl { get; set; } = "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiLogin { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public bool TestMode { get; set; } = true;

    /// <summary>Country code sent to PayU (e.g. "PE", "CO"). Must match the account/acquirer.</summary>
    public string PaymentCountry { get; set; } = "PE";

    /// <summary>
    ///     Optional currency override sent to PayU. Empty => use the payment's own currency.
    ///     The public sandbox account is Colombia (COP); set "COP" there so test charges approve.
    /// </summary>
    public string CurrencyOverride { get; set; } = string.Empty;
}
