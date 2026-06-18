using SnapSharp.Application.Contracts.Account;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class AccountService : IAccountService
{
    private const string BalancePath = "v1.0/balance-inquiry";
    private const string InquiryPath = "v1.0/account-inquiry";
    private readonly ISnapSharpMessageSender _sender;

    public AccountService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<AccountBalanceResponse> GetBalanceAsync(
        AccountBalanceRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<AccountBalanceResponse>(
            HttpMethod.Post, BalancePath, request, ct);
    }

    public async Task<AccountRegistrationResponse> InquiryAsync(
        AccountRegistrationRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<AccountRegistrationResponse>(
            HttpMethod.Post, InquiryPath, request, ct);
    }
}
