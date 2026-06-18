using SnapSharp.Models;

namespace SnapSharp.Services;

internal sealed class QrisService : IQrisService
{
    private const string GeneratePath = "v1.0/qr/qr-generate";
    private const string PaymentNotifyPath = "v1.0/qr/qr-payment";

    private readonly SnapSharpClient _client;

    public QrisService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<QrisGenerateResponse> GenerateAsync(
        QrisGenerateRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<QrisGenerateResponse>(
            HttpMethod.Post, GeneratePath, request, ct).ConfigureAwait(false);
    }

    public QrisGenerateResponse Generate(QrisGenerateRequest request)
    {
        return SyncHelper.Run(() => GenerateAsync(request));
    }

    public async Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(
        QrisPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<QrisPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct).ConfigureAwait(false);
    }

    public QrisPaymentNotifyResponse PaymentNotify(QrisPaymentNotifyRequest request)
    {
        return SyncHelper.Run(() => PaymentNotifyAsync(request));
    }
}
