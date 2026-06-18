using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class DirectDebitService : IDirectDebitService
{
    private const string RegisterPath = "v1.0/debit/registration";
    private const string PaymentPath = "v1.0/debit/payment";
    private readonly ISnapSharpMessageSender _sender;

    public DirectDebitService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<DirectDebitRegisterResponse> RegisterAsync(
        DirectDebitRegisterRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<DirectDebitRegisterResponse>(
            HttpMethod.Post, RegisterPath, request, ct);
    }

    public async Task<DirectDebitPaymentResponse> PaymentAsync(
        DirectDebitPaymentRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<DirectDebitPaymentResponse>(
            HttpMethod.Post, PaymentPath, request, ct);
    }
}
