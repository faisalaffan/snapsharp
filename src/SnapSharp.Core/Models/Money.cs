using System.Text.Json.Serialization;

namespace SnapSharp.Models;

public sealed class Money
{
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";
}