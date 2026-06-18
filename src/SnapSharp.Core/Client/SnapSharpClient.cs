using SnapSharp.Application.Interfaces;
using SnapSharp.Infrastructure.Token;

namespace SnapSharp;

public sealed class SnapSharpClient : ISnapSharpClient
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;

    public IAuthService Auth { get; }
    public IAccountService Account { get; }
    public ITransferService Transfer { get; }
    public IVirtualAccountService VirtualAccount { get; }
    public IQrisService Qris { get; }
    public IDirectDebitService DirectDebit { get; }

    public SnapSharpClient(
        IAuthService auth,
        IAccountService account,
        ITransferService transfer,
        IVirtualAccountService virtualAccount,
        IQrisService qris,
        IDirectDebitService directDebit,
        HttpClient httpClient,
        TokenManager tokenManager)
    {
        Auth = auth;
        Account = account;
        Transfer = transfer;
        VirtualAccount = virtualAccount;
        Qris = qris;
        DirectDebit = directDebit;
        _httpClient = httpClient;
        _tokenManager = tokenManager;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _tokenManager.Dispose();
    }
}
