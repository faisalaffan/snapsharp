using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class QrisService : IQrisService
{
    private const string GeneratePath = "v1.0/qr/qr-generate";
    private const string PaymentNotifyPath = "v1.0/qr/qr-payment";
    private readonly ISnapSharpMessageSender _sender;

    public QrisService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<QrisGenerateResponse> GenerateAsync(
        QrisGenerateRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<QrisGenerateResponse>(
            HttpMethod.Post, GeneratePath, request, ct);
    }

    public async Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(
        QrisPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<QrisPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct);
    }
}
