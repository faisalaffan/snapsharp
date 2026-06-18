using SnapSharp.Application.Contracts.Account;

namespace SnapSharp.Application.Interfaces;

public interface IAccountService
{
    Task<AccountBalanceResponse> GetBalanceAsync(AccountBalanceRequest request, CancellationToken ct = default);
    Task<AccountRegistrationResponse> InquiryAsync(AccountRegistrationRequest request, CancellationToken ct = default);
}
