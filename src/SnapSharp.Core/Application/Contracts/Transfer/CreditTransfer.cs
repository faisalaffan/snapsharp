using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Transfer;

public sealed class CreditTransferRequest : BaseRequest
{
    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("sourceAccountNo")]
    public required string SourceAccountNo { get; init; }

    [JsonPropertyName("beneficiaryAccountNo")]
    public required string BeneficiaryAccountNo { get; init; }

    [JsonPropertyName("beneficiaryBankCode")]
    public required string BeneficiaryBankCode { get; init; }

    [JsonPropertyName("beneficiaryName")]
    public required string BeneficiaryName { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("remark")]
    public string? Remark { get; init; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; } = "internal";

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class CreditTransferResponse : BaseResponse
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("sourceAccountNo")]
    public string SourceAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("beneficiaryAccountNo")]
    public string BeneficiaryAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("fee")]
    public Money? Fee { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
