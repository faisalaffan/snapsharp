using SnapSharp.Models;

namespace SnapSharp.Services;

public interface IAccountService
{
    Task<AccountBalanceResponse> GetBalanceAsync(AccountBalanceRequest request, CancellationToken ct = default);
    AccountBalanceResponse GetBalance(AccountBalanceRequest request);
    Task<AccountRegistrationResponse> InquiryAsync(AccountRegistrationRequest request, CancellationToken ct = default);
    AccountRegistrationResponse Inquiry(AccountRegistrationRequest request);
}

public interface ITransferService
{
    Task<CreditTransferResponse> CreditTransferAsync(CreditTransferRequest request, CancellationToken ct = default);
    CreditTransferResponse CreditTransfer(CreditTransferRequest request);
    Task<TransactionHistoryResponse> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default);
    TransactionHistoryResponse GetHistory(TransactionHistoryRequest request);
}

public interface IVirtualAccountService
{
    Task<CreateVaResponse> CreateAsync(CreateVaRequest request, CancellationToken ct = default);
    CreateVaResponse Create(CreateVaRequest request);
    Task<VAInquiryResponse> InquiryAsync(VAInquiryRequest request, CancellationToken ct = default);
    VAInquiryResponse Inquiry(VAInquiryRequest request);
    Task<VAPaymentNotifyResponse> PaymentNotifyAsync(VAPaymentNotifyRequest request, CancellationToken ct = default);
    VAPaymentNotifyResponse PaymentNotify(VAPaymentNotifyRequest request);
}

public interface IQrisService
{
    Task<QrisGenerateResponse> GenerateAsync(QrisGenerateRequest request, CancellationToken ct = default);
    QrisGenerateResponse Generate(QrisGenerateRequest request);
    Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(QrisPaymentNotifyRequest request, CancellationToken ct = default);
    QrisPaymentNotifyResponse PaymentNotify(QrisPaymentNotifyRequest request);
}

public interface IDirectDebitService
{
    Task<DirectDebitRegisterResponse> RegisterAsync(DirectDebitRegisterRequest request, CancellationToken ct = default);
    DirectDebitRegisterResponse Register(DirectDebitRegisterRequest request);
    Task<DirectDebitPaymentResponse> PaymentAsync(DirectDebitPaymentRequest request, CancellationToken ct = default);
    DirectDebitPaymentResponse Payment(DirectDebitPaymentRequest request);
}
