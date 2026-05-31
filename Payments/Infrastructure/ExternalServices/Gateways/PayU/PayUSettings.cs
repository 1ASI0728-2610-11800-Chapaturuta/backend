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
}
