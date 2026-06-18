using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class VirtualAccountService : IVirtualAccountService
{
    private const string CreatePath = "v1.0/transfer-va/create-va";
    private const string InquiryPath = "v1.0/transfer-va/inquiry";
    private const string PaymentNotifyPath = "v1.0/transfer-va/payment";
    private readonly ISnapSharpMessageSender _sender;

    public VirtualAccountService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<CreateVaResponse> CreateAsync(
        CreateVaRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<CreateVaResponse>(
            HttpMethod.Post, CreatePath, request, ct);
    }

    public async Task<VAInquiryResponse> InquiryAsync(
        VAInquiryRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<VAInquiryResponse>(
            HttpMethod.Post, InquiryPath, request, ct);
    }

    public async Task<VAPaymentNotifyResponse> PaymentNotifyAsync(
        VAPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<VAPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct);
    }
}
