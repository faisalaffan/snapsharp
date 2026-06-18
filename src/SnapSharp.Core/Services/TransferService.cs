using SnapSharp.Models;

namespace SnapSharp.Services;

internal sealed class TransferService : ITransferService
{
    private const string CreditTransferPath = "v1.0/transfer-va/credit";
    private const string HistoryPath = "v1.0/transaction-history";

    private readonly SnapSharpClient _client;

    public TransferService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<CreditTransferResponse> CreditTransferAsync(
        CreditTransferRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<CreditTransferResponse>(
            HttpMethod.Post, CreditTransferPath, request, ct).ConfigureAwait(false);
    }

    public CreditTransferResponse CreditTransfer(CreditTransferRequest request)
    {
        return SyncHelper.Run(() => CreditTransferAsync(request));
    }

    public async Task<TransactionHistoryResponse> GetHistoryAsync(
        TransactionHistoryRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<TransactionHistoryResponse>(
            HttpMethod.Post, HistoryPath, request, ct).ConfigureAwait(false);
    }

    public TransactionHistoryResponse GetHistory(TransactionHistoryRequest request)
    {
        return SyncHelper.Run(() => GetHistoryAsync(request));
    }
}
