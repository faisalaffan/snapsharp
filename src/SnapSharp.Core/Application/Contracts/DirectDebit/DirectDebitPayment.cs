using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.DirectDebit;

public sealed class DirectDebitPaymentRequest : BaseRequest
{
    [JsonPropertyName("registrationId")]
    public required string RegistrationId { get; init; }

    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class DirectDebitPaymentResponse : BaseResponse
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("registrationId")]
    public string RegistrationId { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }
}
