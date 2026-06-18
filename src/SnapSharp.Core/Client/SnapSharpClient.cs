using SnapSharp.Application.Interfaces;

namespace SnapSharp;

public sealed class SnapSharpClient : ISnapSharpClient
{
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
        IDirectDebitService directDebit)
    {
        Auth = auth;
        Account = account;
        Transfer = transfer;
        VirtualAccount = virtualAccount;
        Qris = qris;
        DirectDebit = directDebit;
    }

    public void Dispose() { }
}
