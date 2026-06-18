using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class TransferService : ITransferService
{
    private const string CreditTransferPath = "v1.0/transfer-va/credit";
    private const string HistoryPath = "v1.0/transaction-history";
    private readonly ISnapSharpMessageSender _sender;

    public TransferService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<CreditTransferResponse> CreditTransferAsync(
        CreditTransferRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<CreditTransferResponse>(
            HttpMethod.Post, CreditTransferPath, request, ct);
    }

    public async Task<TransactionHistoryResponse> GetHistoryAsync(
        TransactionHistoryRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<TransactionHistoryResponse>(
            HttpMethod.Post, HistoryPath, request, ct);
    }
}
