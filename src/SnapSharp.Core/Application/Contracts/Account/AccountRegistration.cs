using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;

namespace SnapSharp.Application.Contracts.Account;

public sealed class AccountRegistrationRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class AccountRegistrationResponse : BaseResponse
{
    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("accountType")]
    public string AccountType { get; init; } = string.Empty;

    [JsonPropertyName("accountStatus")]
    public string AccountStatus { get; init; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; init; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTimeOffset RegistrationDate { get; init; }
}
