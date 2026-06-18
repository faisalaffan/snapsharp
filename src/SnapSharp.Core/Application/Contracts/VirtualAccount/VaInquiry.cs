using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class VAInquiryRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public required string VirtualAccountNo { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VAInquiryResponse : BaseResponse
{
    [JsonPropertyName("partnerServiceId")]
    public string PartnerServiceId { get; init; } = string.Empty;

    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountNo")]
    public string VirtualAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountName")]
    public string VirtualAccountName { get; init; } = string.Empty;

    [JsonPropertyName("trxId")]
    public string TrxId { get; init; } = string.Empty;

    [JsonPropertyName("totalAmount")]
    public Money? TotalAmount { get; init; }

    [JsonPropertyName("billDetails")]
    public List<VABillDetail>? BillDetails { get; init; }

    [JsonPropertyName("vaStatus")]
    public string VaStatus { get; init; } = string.Empty;

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }
}
