using SnapSharp.Application.Contracts.Account;
using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Extensions;

public static class ServiceSyncExtensions
{
    public static AccountBalanceResponse GetBalance(this IAccountService svc, AccountBalanceRequest request)
        => Task.Run(() => svc.GetBalanceAsync(request)).GetAwaiter().GetResult();

    public static AccountRegistrationResponse Inquiry(this IAccountService svc, AccountRegistrationRequest request)
        => Task.Run(() => svc.InquiryAsync(request)).GetAwaiter().GetResult();

    public static CreditTransferResponse CreditTransfer(this ITransferService svc, CreditTransferRequest request)
        => Task.Run(() => svc.CreditTransferAsync(request)).GetAwaiter().GetResult();

    public static TransactionHistoryResponse GetHistory(this ITransferService svc, TransactionHistoryRequest request)
        => Task.Run(() => svc.GetHistoryAsync(request)).GetAwaiter().GetResult();

    public static CreateVaResponse Create(this IVirtualAccountService svc, CreateVaRequest request)
        => Task.Run(() => svc.CreateAsync(request)).GetAwaiter().GetResult();

    public static VAInquiryResponse Inquiry(this IVirtualAccountService svc, VAInquiryRequest request)
        => Task.Run(() => svc.InquiryAsync(request)).GetAwaiter().GetResult();

    public static VAPaymentNotifyResponse PaymentNotify(this IVirtualAccountService svc, VAPaymentNotifyRequest request)
        => Task.Run(() => svc.PaymentNotifyAsync(request)).GetAwaiter().GetResult();

    public static QrisGenerateResponse Generate(this IQrisService svc, QrisGenerateRequest request)
        => Task.Run(() => svc.GenerateAsync(request)).GetAwaiter().GetResult();

    public static QrisPaymentNotifyResponse PaymentNotify(this IQrisService svc, QrisPaymentNotifyRequest request)
        => Task.Run(() => svc.PaymentNotifyAsync(request)).GetAwaiter().GetResult();

    public static DirectDebitRegisterResponse Register(this IDirectDebitService svc, DirectDebitRegisterRequest request)
        => Task.Run(() => svc.RegisterAsync(request)).GetAwaiter().GetResult();

    public static DirectDebitPaymentResponse Payment(this IDirectDebitService svc, DirectDebitPaymentRequest request)
        => Task.Run(() => svc.PaymentAsync(request)).GetAwaiter().GetResult();
}
