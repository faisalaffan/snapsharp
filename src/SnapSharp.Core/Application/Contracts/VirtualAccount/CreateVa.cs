using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class CreateVaRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public string? VirtualAccountNo { get; init; }

    [JsonPropertyName("virtualAccountName")]
    public required string VirtualAccountName { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("totalAmount")]
    public required Money TotalAmount { get; init; }

    [JsonPropertyName("billDetails")]
    public List<VABillDetail>? BillDetails { get; init; }

    [JsonPropertyName("expiredDate")]
    public required DateTimeOffset ExpiredDate { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VABillDetail
{
    [JsonPropertyName("billCode")]
    public required string BillCode { get; init; }

    [JsonPropertyName("billName")]
    public required string BillName { get; init; }

    [JsonPropertyName("billAmount")]
    public required Money BillAmount { get; init; }
}

public sealed class CreateVaResponse : BaseResponse
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

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }

    [JsonPropertyName("vaStatus")]
    public string VaStatus { get; init; } = string.Empty;
}
