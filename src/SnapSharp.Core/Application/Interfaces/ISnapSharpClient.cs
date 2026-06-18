namespace SnapSharp.Application.Interfaces;

public interface ISnapSharpClient : IDisposable
{
    IAuthService Auth { get; }
    IAccountService Account { get; }
    ITransferService Transfer { get; }
    IVirtualAccountService VirtualAccount { get; }
    IQrisService Qris { get; }
    IDirectDebitService DirectDebit { get; }
}
