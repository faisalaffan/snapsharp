using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Auth;

public sealed record AccessTokenResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; init; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; init; } = string.Empty;

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("tokenType")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }

    public DateTime ExpiresAt { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
