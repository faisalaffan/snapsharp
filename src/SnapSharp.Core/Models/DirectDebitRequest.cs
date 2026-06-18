using System.Text.Json.Serialization;

namespace SnapSharp.Models;

public sealed class DirectDebitRegisterRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("merchantName")]
    public required string MerchantName { get; init; }

    [JsonPropertyName("validPeriod")]
    public string? ValidPeriod { get; init; }

    [JsonPropertyName("maxAmount")]
    public required Money MaxAmount { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class DirectDebitRegisterResponse : BaseResponse
{
    [JsonPropertyName("registrationId")]
    public string RegistrationId { get; init; } = string.Empty;

    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("merchantId")]
    public string MerchantId { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("validFrom")]
    public DateTimeOffset ValidFrom { get; init; }

    [JsonPropertyName("validUntil")]
    public DateTimeOffset ValidUntil { get; init; }
}

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
