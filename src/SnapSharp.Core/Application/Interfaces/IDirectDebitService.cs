using SnapSharp.Application.Contracts.DirectDebit;

namespace SnapSharp.Application.Interfaces;

public interface IDirectDebitService
{
    Task<DirectDebitRegisterResponse> RegisterAsync(DirectDebitRegisterRequest request, CancellationToken ct = default);
    Task<DirectDebitPaymentResponse> PaymentAsync(DirectDebitPaymentRequest request, CancellationToken ct = default);
}
