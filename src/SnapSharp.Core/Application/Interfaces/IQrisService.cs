using SnapSharp.Application.Contracts.Qris;

namespace SnapSharp.Application.Interfaces;

public interface IQrisService
{
    Task<QrisGenerateResponse> GenerateAsync(QrisGenerateRequest request, CancellationToken ct = default);
    Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(QrisPaymentNotifyRequest request, CancellationToken ct = default);
}
