using System.Text.Json.Serialization;

namespace SnapSharp.Models;

public sealed class AccountBalanceRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("balanceType")]
    public string? BalanceType { get; init; }
}

public sealed class AccountBalanceResponse : BaseResponse
{
    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("availableBalance")]
    public Money? AvailableBalance { get; init; }

    [JsonPropertyName("ledgerBalance")]
    public Money? LedgerBalance { get; init; }

    [JsonPropertyName("holdAmount")]
    public Money? HoldAmount { get; init; }

    [JsonPropertyName("accountStatus")]
    public string AccountStatus { get; init; } = string.Empty;
}
