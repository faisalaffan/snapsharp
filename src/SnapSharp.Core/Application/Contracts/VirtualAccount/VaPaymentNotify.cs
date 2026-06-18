using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class VAPaymentNotifyRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public required string VirtualAccountNo { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("paymentRequestId")]
    public required string PaymentRequestId { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("paidAmount")]
    public required Money PaidAmount { get; init; }

    [JsonPropertyName("paidBills")]
    public List<VAPaidBill>? PaidBills { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VAPaidBill
{
    [JsonPropertyName("billCode")]
    public required string BillCode { get; init; }

    [JsonPropertyName("paidAmount")]
    public required Money PaidAmount { get; init; }
}

public sealed class VAPaymentNotifyResponse : BaseResponse
{
}
