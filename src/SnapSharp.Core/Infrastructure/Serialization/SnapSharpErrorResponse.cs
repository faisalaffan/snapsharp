using System.Text.Json.Serialization;

namespace SnapSharp.Infrastructure.Serialization;

internal sealed class SnapSharpErrorResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;
}
