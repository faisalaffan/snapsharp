using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Qris;

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
