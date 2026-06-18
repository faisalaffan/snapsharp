using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapSharp;

[JsonSerializable(typeof(SnapSharpErrorResponse))]
[JsonSerializable(typeof(Models.AccessTokenResponse))]
[JsonSerializable(typeof(Models.Money))]
[JsonSerializable(typeof(Models.B2b2cTokenRequest))]
[JsonSerializable(typeof(Models.AccountBalanceRequest))]
[JsonSerializable(typeof(Models.AccountBalanceResponse))]
[JsonSerializable(typeof(Models.AccountRegistrationRequest))]
[JsonSerializable(typeof(Models.AccountRegistrationResponse))]
[JsonSerializable(typeof(Models.TransactionHistoryRequest))]
[JsonSerializable(typeof(Models.TransactionHistoryResponse))]
[JsonSerializable(typeof(Models.TransactionEntry))]
[JsonSerializable(typeof(Models.CreditTransferRequest))]
[JsonSerializable(typeof(Models.CreditTransferResponse))]
[JsonSerializable(typeof(Models.CreateVaRequest))]
[JsonSerializable(typeof(Models.VABillDetail))]
[JsonSerializable(typeof(Models.CreateVaResponse))]
[JsonSerializable(typeof(Models.VAInquiryRequest))]
[JsonSerializable(typeof(Models.VAInquiryResponse))]
[JsonSerializable(typeof(Models.VAPaymentNotifyRequest))]
[JsonSerializable(typeof(Models.VAPaidBill))]
[JsonSerializable(typeof(Models.VAPaymentNotifyResponse))]
[JsonSerializable(typeof(Models.QrisGenerateRequest))]
[JsonSerializable(typeof(Models.QrisGenerateResponse))]
[JsonSerializable(typeof(Models.QrisPaymentNotifyRequest))]
[JsonSerializable(typeof(Models.QrisPaymentNotifyResponse))]
[JsonSerializable(typeof(Models.DirectDebitRegisterRequest))]
[JsonSerializable(typeof(Models.DirectDebitRegisterResponse))]
[JsonSerializable(typeof(Models.DirectDebitPaymentRequest))]
[JsonSerializable(typeof(Models.DirectDebitPaymentResponse))]
internal partial class SnapSharpJsonContext : JsonSerializerContext
{
    internal static SnapSharpJsonContext Default { get; } = new(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    });
}

internal sealed class SnapSharpErrorResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;
}
