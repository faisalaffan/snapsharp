using SnapSharp.Application.Contracts.Transfer;

namespace SnapSharp.Application.Interfaces;

public interface ITransferService
{
    Task<CreditTransferResponse> CreditTransferAsync(CreditTransferRequest request, CancellationToken ct = default);
    Task<TransactionHistoryResponse> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default);
}
