using SnapSharp.Models;

namespace SnapSharp.Services;

internal sealed class AccountService : IAccountService
{
    private const string BalancePath = "v1.0/balance-inquiry";
    private const string InquiryPath = "v1.0/account-inquiry";

    private readonly SnapSharpClient _client;

    public AccountService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<AccountBalanceResponse> GetBalanceAsync(
        AccountBalanceRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<AccountBalanceResponse>(
            HttpMethod.Post, BalancePath, request, ct).ConfigureAwait(false);
    }

    public AccountBalanceResponse GetBalance(AccountBalanceRequest request)
    {
        return SyncHelper.Run(() => GetBalanceAsync(request));
    }

    public async Task<AccountRegistrationResponse> InquiryAsync(
        AccountRegistrationRequest request, CancellationToken ct = default)
    {
        return await _client.SendAsync<AccountRegistrationResponse>(
            HttpMethod.Post, InquiryPath, request, ct).ConfigureAwait(false);
    }

    public AccountRegistrationResponse Inquiry(AccountRegistrationRequest request)
    {
        return SyncHelper.Run(() => InquiryAsync(request));
    }
}
