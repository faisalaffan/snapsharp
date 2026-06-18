using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Qris;

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
