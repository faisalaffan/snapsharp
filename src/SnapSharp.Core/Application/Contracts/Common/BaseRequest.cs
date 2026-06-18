using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Common;

public abstract class BaseRequest
{
    [JsonPropertyName("partnerReferenceNo")]
    public string PartnerReferenceNo { get; init; } = Guid.NewGuid().ToString();
}

public abstract class BaseResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; init; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; init; } = string.Empty;
}
