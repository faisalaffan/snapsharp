using SnapSharp.Models;

namespace SnapSharp.Services;

internal sealed class VirtualAccountService : IVirtualAccountService
{
    private const string CreatePath = "v1.0/transfer-va/create-va";
    private const string InquiryPath = "v1.0/transfer-va/inquiry";
    private const string PaymentNotifyPath = "v1.0/transfer-va/payment";

    private readonly SnapSharpClient _client;

    public VirtualAccountService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<CreateVaResponse> CreateAsync(
        CreateVaRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<CreateVaResponse>(
            HttpMethod.Post, CreatePath, request, ct).ConfigureAwait(false);
    }

    public CreateVaResponse Create(CreateVaRequest request)
    {
        return SyncHelper.Run(() => CreateAsync(request));
    }

    public async Task<VAInquiryResponse> InquiryAsync(
        VAInquiryRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<VAInquiryResponse>(
            HttpMethod.Post, InquiryPath, request, ct).ConfigureAwait(false);
    }

    public VAInquiryResponse Inquiry(VAInquiryRequest request)
    {
        return SyncHelper.Run(() => InquiryAsync(request));
    }

    public async Task<VAPaymentNotifyResponse> PaymentNotifyAsync(
        VAPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<VAPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct).ConfigureAwait(false);
    }

    public VAPaymentNotifyResponse PaymentNotify(VAPaymentNotifyRequest request)
    {
        return SyncHelper.Run(() => PaymentNotifyAsync(request));
    }
}
