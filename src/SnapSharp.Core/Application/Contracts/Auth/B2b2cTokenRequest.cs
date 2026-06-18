using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Auth;

public sealed class B2b2cTokenRequest
{
    [JsonPropertyName("grantType")]
    public string GrantType { get; init; } = "client_credentials";

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}
