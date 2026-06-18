using System.Text.Json;
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Account;
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Infrastructure.Serialization;

[JsonSerializable(typeof(SnapSharpErrorResponse))]
[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSerializable(typeof(Money))]
[JsonSerializable(typeof(B2b2cTokenRequest))]
[JsonSerializable(typeof(AccountBalanceRequest))]
[JsonSerializable(typeof(AccountBalanceResponse))]
[JsonSerializable(typeof(AccountRegistrationRequest))]
[JsonSerializable(typeof(AccountRegistrationResponse))]
[JsonSerializable(typeof(TransactionHistoryRequest))]
[JsonSerializable(typeof(TransactionHistoryResponse))]
[JsonSerializable(typeof(TransactionEntry))]
[JsonSerializable(typeof(CreditTransferRequest))]
[JsonSerializable(typeof(CreditTransferResponse))]
[JsonSerializable(typeof(CreateVaRequest))]
[JsonSerializable(typeof(VABillDetail))]
[JsonSerializable(typeof(CreateVaResponse))]
[JsonSerializable(typeof(VAInquiryRequest))]
[JsonSerializable(typeof(VAInquiryResponse))]
[JsonSerializable(typeof(VAPaymentNotifyRequest))]
[JsonSerializable(typeof(VAPaidBill))]
[JsonSerializable(typeof(VAPaymentNotifyResponse))]
[JsonSerializable(typeof(QrisGenerateRequest))]
[JsonSerializable(typeof(QrisGenerateResponse))]
[JsonSerializable(typeof(QrisPaymentNotifyRequest))]
[JsonSerializable(typeof(QrisPaymentNotifyResponse))]
[JsonSerializable(typeof(DirectDebitRegisterRequest))]
[JsonSerializable(typeof(DirectDebitRegisterResponse))]
[JsonSerializable(typeof(DirectDebitPaymentRequest))]
[JsonSerializable(typeof(DirectDebitPaymentResponse))]
internal partial class SnapSharpJsonContext : JsonSerializerContext
{
    internal static SnapSharpJsonContext Instance { get; } = new(new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    });
}
