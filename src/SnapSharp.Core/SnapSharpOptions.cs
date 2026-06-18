namespace SnapSharp;

public sealed class SnapSharpOptions
{
    public required string BaseUrl { get; init; }
    public required string ClientId { get; init; }
    public required string PrivateKeyPem { get; init; }
    public required string ChannelId { get; init; }
    public required string PartnerId { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
}