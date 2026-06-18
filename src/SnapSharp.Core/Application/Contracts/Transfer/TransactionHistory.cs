using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Transfer;

public sealed class TransactionHistoryRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("fromDateTime")]
    public required DateTimeOffset FromDateTime { get; init; }

    [JsonPropertyName("toDateTime")]
    public required DateTimeOffset ToDateTime { get; init; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; init; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; } = 20;
}

public sealed class TransactionHistoryResponse : BaseResponse
{
    [JsonPropertyName("transactions")]
    public List<TransactionEntry> Transactions { get; init; } = [];

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; init; }
}

public sealed class TransactionEntry
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("referenceNo")]
    public string ReferenceNo { get; init; } = string.Empty;

    [JsonPropertyName("runningBalance")]
    public Money? RunningBalance { get; init; }
}
