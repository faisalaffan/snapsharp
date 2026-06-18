using System.Text.Json.Serialization;

namespace SnapSharp.Models;

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
