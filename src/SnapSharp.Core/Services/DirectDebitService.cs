using SnapSharp.Models;

namespace SnapSharp.Services;

internal sealed class DirectDebitService : IDirectDebitService
{
    private const string RegisterPath = "v1.0/debit/registration";
    private const string PaymentPath = "v1.0/debit/payment";

    private readonly SnapSharpClient _client;

    public DirectDebitService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<DirectDebitRegisterResponse> RegisterAsync(
        DirectDebitRegisterRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<DirectDebitRegisterResponse>(
            HttpMethod.Post, RegisterPath, request, ct).ConfigureAwait(false);
    }

    public DirectDebitRegisterResponse Register(DirectDebitRegisterRequest request)
    {
        return SyncHelper.Run(() => RegisterAsync(request));
    }

    public async Task<DirectDebitPaymentResponse> PaymentAsync(
        DirectDebitPaymentRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<DirectDebitPaymentResponse>(
            HttpMethod.Post, PaymentPath, request, ct).ConfigureAwait(false);
    }

    public DirectDebitPaymentResponse Payment(DirectDebitPaymentRequest request)
    {
        return SyncHelper.Run(() => PaymentAsync(request));
    }
}
