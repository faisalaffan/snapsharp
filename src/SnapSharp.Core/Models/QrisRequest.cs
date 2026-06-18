using System.Text.Json.Serialization;

namespace SnapSharp.Models;

public sealed class QrisGenerateRequest : BaseRequest
{
    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("goodsType")]
    public string? GoodsType { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("storeId")]
    public required string StoreId { get; init; }

    [JsonPropertyName("validityPeriod")]
    public int ValidityPeriod { get; init; } = 300;

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class QrisGenerateResponse : BaseResponse
{
    [JsonPropertyName("qrCodeData")]
    public string QrCodeData { get; init; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; init; } = string.Empty;

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }
}

public sealed class QrisPaymentNotifyRequest : BaseRequest
{
    [JsonPropertyName("originalPartnerReferenceNo")]
    public required string OriginalPartnerReferenceNo { get; init; }

    [JsonPropertyName("originalReferenceNo")]
    public required string OriginalReferenceNo { get; init; }

    [JsonPropertyName("transactionDate")]
    public required DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("storeId")]
    public string? StoreId { get; init; }

    [JsonPropertyName("terminalId")]
    public string? TerminalId { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class QrisPaymentNotifyResponse : BaseResponse
{
}
