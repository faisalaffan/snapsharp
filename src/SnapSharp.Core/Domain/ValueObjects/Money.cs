using System.Text.Json.Serialization;

namespace SnapSharp.Domain.ValueObjects;

public sealed class Money
{
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";
}
