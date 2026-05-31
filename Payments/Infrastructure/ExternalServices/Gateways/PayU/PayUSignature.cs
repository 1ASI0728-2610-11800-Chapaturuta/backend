using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Frock_backend.Payments.Infrastructure.ExternalServices.Gateways.PayU;

public static class PayUSignature
{
    public static string ForRequest(string apiKey, string merchantId, string referenceCode, decimal amount, string currency)
    {
        var raw = $"{apiKey}~{merchantId}~{referenceCode}~{amount.ToString("0.00", CultureInfo.InvariantCulture)}~{currency}";
        return Md5Hex(raw);
    }

    public static string ForWebhook(string apiKey, string merchantId, string referenceSale, string amountFormatted, string currency, string statePol)
    {
        var raw = $"{apiKey}~{merchantId}~{referenceSale}~{amountFormatted}~{currency}~{statePol}";
        return Md5Hex(raw);
    }

    private static string Md5Hex(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        return sb.ToString();
    }
}
