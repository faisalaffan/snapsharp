# SnapSharp Clean Architecture Refactor — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Refactor SnapSharp.Core ke clean architecture 4-layer (Domain, Application, Infrastructure, Client) dengan strict namespace, single-project, DI-ready, testable.

**Architecture:** Single `.csproj` project dengan folder-based namespace enforcement. Layer: Domain (no deps) → Application.Contracts + Application.Interfaces → Application.Services → Infrastructure → Client (facade). Semua service depend ke `ISnapSharpMessageSender` (abstraction), bukan concrete `SnapSharpClient`. TokenManager terisolasi untuk menghindari circular dependency.

**Tech Stack:** C# 12, .NET 8, System.Text.Json source generator, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Http

## Global Constraints

- Target framework: net8.0
- Nullable: enable
- ImplicitUsings: enable
- Single project: SnapSharp.Core.csproj (tidak tambah project baru)
- Tidak mengubah behavior API calls
- Tidak menambah fitur baru
- `ISnapSharpClient` dan service interfaces tetap public dengan namespace yang bisa di-access
- Sync methods dihapus dari interface, jadi extension methods
- Semua `ConfigureAwait(false)` dihapus
- TokenManager tidak boleh depend ke IAuthService (circular dep avoidance)

---

### Task 1: Create folder structure and Domain layer

**Files:**
- Create: `src/SnapSharp.Core/Domain/Enums/PaymentEnums.cs`
- Create: `src/SnapSharp.Core/Domain/ValueObjects/Money.cs`
- Create: `src/SnapSharp.Core/Domain/Options/SnapSharpOptions.cs`
- Create: `src/SnapSharp.Core/Domain/Exceptions/SnapSharpException.cs`

**Interfaces:**
- Produces: `SnapSharp.Domain.PaymentEnums` (AccountType, TransactionStatus, TransferStatus, VaStatus, PaymentMethod)
- Produces: `SnapSharp.Domain.Money` class (Value, Currency)
- Produces: `SnapSharp.Domain.SnapSharpOptions` class (BaseUrl, ClientId, PrivateKeyPem, ChannelId, PartnerId, TimeoutSeconds, MaxRetries)
- Produces: `SnapSharp.Domain.SnapSharpException`, `SnapSharpAuthenticationException`, `SnapSharpApiException`, `SnapSharpSignatureException`

- [ ] **Step 1: Create Domain/Enums/PaymentEnums.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType { Savings, Current, Loan, TimeDeposit, Investment, Other }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionStatus { Pending, Success, Failed, Reversed, Refunded }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferStatus { Pending, Success, Failed, Reversed }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VaStatus { Active, Inactive, Expired, Paid, Cancelled }

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PaymentMethod { VirtualAccount, Qris, DirectDebit, Transfer }
```

- [ ] **Step 2: Create Domain/ValueObjects/Money.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Domain.ValueObjects;

public sealed class Money
{
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";
}
```

- [ ] **Step 3: Create Domain/Options/SnapSharpOptions.cs**

```csharp
namespace SnapSharp.Domain.Options;

public sealed class SnapSharpOptions
{
    public required string BaseUrl { get; init; }
    public required string ClientId { get; init; }
    public required string PrivateKeyPem { get; init; }
    public required string ChannelId { get; init; }
    public required string PartnerId { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxRetries { get; init; } = 3;
}
```

- [ ] **Step 4: Create Domain/Exceptions/SnapSharpException.cs**

```csharp
namespace SnapSharp.Domain.Exceptions;

public class SnapSharpException : Exception
{
    public string? ResponseCode { get; }
    public string? ResponseMessage { get; }

    public SnapSharpException(string message) : base(message) { }
    public SnapSharpException(string message, Exception innerException) : base(message, innerException) { }
    public SnapSharpException(string responseCode, string responseMessage)
        : base($"[{responseCode}] {responseMessage}")
    { ResponseCode = responseCode; ResponseMessage = responseMessage; }
}

public class SnapSharpAuthenticationException : SnapSharpException
{
    public SnapSharpAuthenticationException(string message) : base(message) { }
    public SnapSharpAuthenticationException(string responseCode, string responseMessage)
        : base(responseCode, responseMessage) { }
}

public class SnapSharpApiException : SnapSharpException
{
    public int HttpStatusCode { get; }
    public SnapSharpApiException(int httpStatusCode, string responseCode, string responseMessage)
        : base(responseCode, responseMessage) { HttpStatusCode = httpStatusCode; }
}

public class SnapSharpSignatureException : SnapSharpException
{
    public SnapSharpSignatureException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

- [ ] **Step 5: Commit**

```bash
git add src/SnapSharp.Core/Domain/
git commit -m "refactor: create Domain layer with enums, value objects, options, exceptions"
```

---

### Task 2: Create Application Contracts layer

**Files:**
- Create: `src/SnapSharp.Core/Application/Contracts/Common/BaseRequest.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Auth/AccessTokenResponse.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Auth/B2b2cTokenRequest.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Account/AccountBalance.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Account/AccountRegistration.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Transfer/CreditTransfer.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Transfer/TransactionHistory.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/VirtualAccount/CreateVa.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/VirtualAccount/VaInquiry.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/VirtualAccount/VaPaymentNotify.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Qris/QrisGenerate.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/Qris/QrisPaymentNotify.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/DirectDebit/DirectDebitRegister.cs`
- Create: `src/SnapSharp.Core/Application/Contracts/DirectDebit/DirectDebitPayment.cs`

**Interfaces:**
- Consumes: `SnapSharp.Domain.ValueObjects.Money`
- Produces: Semua DTO classes di `SnapSharp.Application.Contracts.*` namespace

- [ ] **Step 1: Create Contracts/Common/BaseRequest.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Common;

public abstract class BaseRequest
{
    [JsonPropertyName("partnerReferenceNo")]
    public string PartnerReferenceNo { get; init; } = Guid.NewGuid().ToString();
}

public abstract class BaseResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; init; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; init; } = string.Empty;
}
```

- [ ] **Step 2: Create Contracts/Auth/AccessTokenResponse.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Auth;

public sealed record AccessTokenResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; init; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; init; } = string.Empty;

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("tokenType")]
    public string TokenType { get; init; } = "Bearer";

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }

    public DateTime ExpiresAt { get; init; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
```

- [ ] **Step 3: Create Contracts/Auth/B2b2cTokenRequest.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Application.Contracts.Auth;

public sealed class B2b2cTokenRequest
{
    [JsonPropertyName("grantType")]
    public string GrantType { get; init; } = "client_credentials";

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}
```

- [ ] **Step 4: Create Contracts/Account/AccountBalance.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Account;

public sealed class AccountBalanceRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("balanceType")]
    public string? BalanceType { get; init; }
}

public sealed class AccountBalanceResponse : BaseResponse
{
    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("availableBalance")]
    public Money? AvailableBalance { get; init; }

    [JsonPropertyName("ledgerBalance")]
    public Money? LedgerBalance { get; init; }

    [JsonPropertyName("holdAmount")]
    public Money? HoldAmount { get; init; }

    [JsonPropertyName("accountStatus")]
    public string AccountStatus { get; init; } = string.Empty;
}
```

- [ ] **Step 5: Create Contracts/Account/AccountRegistration.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;

namespace SnapSharp.Application.Contracts.Account;

public sealed class AccountRegistrationRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class AccountRegistrationResponse : BaseResponse
{
    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("accountType")]
    public string AccountType { get; init; } = string.Empty;

    [JsonPropertyName("accountStatus")]
    public string AccountStatus { get; init; } = string.Empty;

    [JsonPropertyName("customerName")]
    public string CustomerName { get; init; } = string.Empty;

    [JsonPropertyName("registrationDate")]
    public DateTimeOffset RegistrationDate { get; init; }
}
```

- [ ] **Step 6: Create Contracts/Transfer/CreditTransfer.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Transfer;

public sealed class CreditTransferRequest : BaseRequest
{
    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("sourceAccountNo")]
    public required string SourceAccountNo { get; init; }

    [JsonPropertyName("beneficiaryAccountNo")]
    public required string BeneficiaryAccountNo { get; init; }

    [JsonPropertyName("beneficiaryBankCode")]
    public required string BeneficiaryBankCode { get; init; }

    [JsonPropertyName("beneficiaryName")]
    public required string BeneficiaryName { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("remark")]
    public string? Remark { get; init; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; } = "internal";

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class CreditTransferResponse : BaseResponse
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("sourceAccountNo")]
    public string SourceAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("beneficiaryAccountNo")]
    public string BeneficiaryAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("fee")]
    public Money? Fee { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}
```

- [ ] **Step 7: Create Contracts/Transfer/TransactionHistory.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Transfer;

public sealed class TransactionHistoryRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("fromDateTime")]
    public required DateTimeOffset FromDateTime { get; init; }

    [JsonPropertyName("toDateTime")]
    public required DateTimeOffset ToDateTime { get; init; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; init; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; } = 20;
}

public sealed class TransactionHistoryResponse : BaseResponse
{
    [JsonPropertyName("transactions")]
    public List<TransactionEntry> Transactions { get; init; } = [];

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    [JsonPropertyName("totalRecords")]
    public int TotalRecords { get; init; }
}

public sealed class TransactionEntry
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("transactionType")]
    public string TransactionType { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("referenceNo")]
    public string ReferenceNo { get; init; } = string.Empty;

    [JsonPropertyName("runningBalance")]
    public Money? RunningBalance { get; init; }
}
```

- [ ] **Step 8: Create Contracts/VirtualAccount/CreateVa.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class CreateVaRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public string? VirtualAccountNo { get; init; }

    [JsonPropertyName("virtualAccountName")]
    public required string VirtualAccountName { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("totalAmount")]
    public required Money TotalAmount { get; init; }

    [JsonPropertyName("billDetails")]
    public List<VABillDetail>? BillDetails { get; init; }

    [JsonPropertyName("expiredDate")]
    public required DateTimeOffset ExpiredDate { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VABillDetail
{
    [JsonPropertyName("billCode")]
    public required string BillCode { get; init; }

    [JsonPropertyName("billName")]
    public required string BillName { get; init; }

    [JsonPropertyName("billAmount")]
    public required Money BillAmount { get; init; }
}

public sealed class CreateVaResponse : BaseResponse
{
    [JsonPropertyName("partnerServiceId")]
    public string PartnerServiceId { get; init; } = string.Empty;

    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountNo")]
    public string VirtualAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountName")]
    public string VirtualAccountName { get; init; } = string.Empty;

    [JsonPropertyName("trxId")]
    public string TrxId { get; init; } = string.Empty;

    [JsonPropertyName("totalAmount")]
    public Money? TotalAmount { get; init; }

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }

    [JsonPropertyName("vaStatus")]
    public string VaStatus { get; init; } = string.Empty;
}
```

- [ ] **Step 9: Create Contracts/VirtualAccount/VaInquiry.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class VAInquiryRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public required string VirtualAccountNo { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VAInquiryResponse : BaseResponse
{
    [JsonPropertyName("partnerServiceId")]
    public string PartnerServiceId { get; init; } = string.Empty;

    [JsonPropertyName("customerNo")]
    public string CustomerNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountNo")]
    public string VirtualAccountNo { get; init; } = string.Empty;

    [JsonPropertyName("virtualAccountName")]
    public string VirtualAccountName { get; init; } = string.Empty;

    [JsonPropertyName("trxId")]
    public string TrxId { get; init; } = string.Empty;

    [JsonPropertyName("totalAmount")]
    public Money? TotalAmount { get; init; }

    [JsonPropertyName("billDetails")]
    public List<VABillDetail>? BillDetails { get; init; }

    [JsonPropertyName("vaStatus")]
    public string VaStatus { get; init; } = string.Empty;

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }
}
```

- [ ] **Step 10: Create Contracts/VirtualAccount/VaPaymentNotify.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.VirtualAccount;

public sealed class VAPaymentNotifyRequest : BaseRequest
{
    [JsonPropertyName("partnerServiceId")]
    public required string PartnerServiceId { get; init; }

    [JsonPropertyName("customerNo")]
    public required string CustomerNo { get; init; }

    [JsonPropertyName("virtualAccountNo")]
    public required string VirtualAccountNo { get; init; }

    [JsonPropertyName("trxId")]
    public required string TrxId { get; init; }

    [JsonPropertyName("paymentRequestId")]
    public required string PaymentRequestId { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("paidAmount")]
    public required Money PaidAmount { get; init; }

    [JsonPropertyName("paidBills")]
    public List<VAPaidBill>? PaidBills { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class VAPaidBill
{
    [JsonPropertyName("billCode")]
    public required string BillCode { get; init; }

    [JsonPropertyName("paidAmount")]
    public required Money PaidAmount { get; init; }
}

public sealed class VAPaymentNotifyResponse : BaseResponse
{
}
```

- [ ] **Step 11: Create Contracts/Qris/QrisGenerate.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Qris;

public sealed class QrisGenerateRequest : BaseRequest
{
    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("goodsType")]
    public string? GoodsType { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("storeId")]
    public required string StoreId { get; init; }

    [JsonPropertyName("validityPeriod")]
    public int ValidityPeriod { get; init; } = 300;

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class QrisGenerateResponse : BaseResponse
{
    [JsonPropertyName("qrCodeData")]
    public string QrCodeData { get; init; } = string.Empty;

    [JsonPropertyName("qrCodeUrl")]
    public string QrCodeUrl { get; init; } = string.Empty;

    [JsonPropertyName("expiredDate")]
    public DateTimeOffset ExpiredDate { get; init; }
}
```

- [ ] **Step 12: Create Contracts/Qris/QrisPaymentNotify.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.Qris;

public sealed class QrisPaymentNotifyRequest : BaseRequest
{
    [JsonPropertyName("originalPartnerReferenceNo")]
    public required string OriginalPartnerReferenceNo { get; init; }

    [JsonPropertyName("originalReferenceNo")]
    public required string OriginalReferenceNo { get; init; }

    [JsonPropertyName("transactionDate")]
    public required DateTimeOffset TransactionDate { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("storeId")]
    public string? StoreId { get; init; }

    [JsonPropertyName("terminalId")]
    public string? TerminalId { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class QrisPaymentNotifyResponse : BaseResponse
{
}
```

- [ ] **Step 13: Create Contracts/DirectDebit/DirectDebitRegister.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.DirectDebit;

public sealed class DirectDebitRegisterRequest : BaseRequest
{
    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("merchantId")]
    public required string MerchantId { get; init; }

    [JsonPropertyName("merchantName")]
    public required string MerchantName { get; init; }

    [JsonPropertyName("validPeriod")]
    public string? ValidPeriod { get; init; }

    [JsonPropertyName("maxAmount")]
    public required Money MaxAmount { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class DirectDebitRegisterResponse : BaseResponse
{
    [JsonPropertyName("registrationId")]
    public string RegistrationId { get; init; } = string.Empty;

    [JsonPropertyName("accountNo")]
    public string AccountNo { get; init; } = string.Empty;

    [JsonPropertyName("merchantId")]
    public string MerchantId { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("validFrom")]
    public DateTimeOffset ValidFrom { get; init; }

    [JsonPropertyName("validUntil")]
    public DateTimeOffset ValidUntil { get; init; }
}
```

- [ ] **Step 14: Create Contracts/DirectDebit/DirectDebitPayment.cs**

```csharp
using System.Text.Json.Serialization;
using SnapSharp.Application.Contracts.Common;
using SnapSharp.Domain.ValueObjects;

namespace SnapSharp.Application.Contracts.DirectDebit;

public sealed class DirectDebitPaymentRequest : BaseRequest
{
    [JsonPropertyName("registrationId")]
    public required string RegistrationId { get; init; }

    [JsonPropertyName("accountNo")]
    public required string AccountNo { get; init; }

    [JsonPropertyName("amount")]
    public required Money Amount { get; init; }

    [JsonPropertyName("currency")]
    public string Currency { get; init; } = "IDR";

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("additionalInfo")]
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

public sealed class DirectDebitPaymentResponse : BaseResponse
{
    [JsonPropertyName("transactionId")]
    public string TransactionId { get; init; } = string.Empty;

    [JsonPropertyName("registrationId")]
    public string RegistrationId { get; init; } = string.Empty;

    [JsonPropertyName("amount")]
    public Money? Amount { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("transactionDate")]
    public DateTimeOffset TransactionDate { get; init; }
}
```

- [ ] **Step 15: Commit**

```bash
git add src/SnapSharp.Core/Application/Contracts/
git commit -m "refactor: create Application Contracts layer with DTOs per service"
```

---

### Task 3: Create Application Interfaces (async-only)

**Files:**
- Create: `src/SnapSharp.Core/Application/Interfaces/ISnapSharpMessageSender.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/IAuthService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/IAccountService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/ITransferService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/IVirtualAccountService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/IQrisService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/IDirectDebitService.cs`
- Create: `src/SnapSharp.Core/Application/Interfaces/ISnapSharpClient.cs`

**Interfaces:**
- Consumes: `SnapSharp.Application.Contracts.*`
- Produces: `ISnapSharpMessageSender`, `IAuthService`, `IAccountService`, `ITransferService`, `IVirtualAccountService`, `IQrisService`, `IDirectDebitService`, `ISnapSharpClient`

- [ ] **Step 1: Create ISnapSharpMessageSender.cs**

```csharp
namespace SnapSharp.Application.Interfaces;

public interface ISnapSharpMessageSender
{
    Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class;
}
```

- [ ] **Step 2: Create IAuthService.cs**

```csharp
using SnapSharp.Application.Contracts.Auth;

namespace SnapSharp.Application.Interfaces;

public interface IAuthService
{
    Task<AccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<AccessTokenResponse> GetAccessTokenB2b2cAsync(
        string customerNo,
        string accountNo,
        Dictionary<string, string>? additionalInfo = null,
        CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Create IAccountService.cs**

```csharp
using SnapSharp.Application.Contracts.Account;

namespace SnapSharp.Application.Interfaces;

public interface IAccountService
{
    Task<AccountBalanceResponse> GetBalanceAsync(AccountBalanceRequest request, CancellationToken ct = default);
    Task<AccountRegistrationResponse> InquiryAsync(AccountRegistrationRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 4: Create ITransferService.cs**

```csharp
using SnapSharp.Application.Contracts.Transfer;

namespace SnapSharp.Application.Interfaces;

public interface ITransferService
{
    Task<CreditTransferResponse> CreditTransferAsync(CreditTransferRequest request, CancellationToken ct = default);
    Task<TransactionHistoryResponse> GetHistoryAsync(TransactionHistoryRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 5: Create IVirtualAccountService.cs**

```csharp
using SnapSharp.Application.Contracts.VirtualAccount;

namespace SnapSharp.Application.Interfaces;

public interface IVirtualAccountService
{
    Task<CreateVaResponse> CreateAsync(CreateVaRequest request, CancellationToken ct = default);
    Task<VAInquiryResponse> InquiryAsync(VAInquiryRequest request, CancellationToken ct = default);
    Task<VAPaymentNotifyResponse> PaymentNotifyAsync(VAPaymentNotifyRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 6: Create IQrisService.cs**

```csharp
using SnapSharp.Application.Contracts.Qris;

namespace SnapSharp.Application.Interfaces;

public interface IQrisService
{
    Task<QrisGenerateResponse> GenerateAsync(QrisGenerateRequest request, CancellationToken ct = default);
    Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(QrisPaymentNotifyRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 7: Create IDirectDebitService.cs**

```csharp
using SnapSharp.Application.Contracts.DirectDebit;

namespace SnapSharp.Application.Interfaces;

public interface IDirectDebitService
{
    Task<DirectDebitRegisterResponse> RegisterAsync(DirectDebitRegisterRequest request, CancellationToken ct = default);
    Task<DirectDebitPaymentResponse> PaymentAsync(DirectDebitPaymentRequest request, CancellationToken ct = default);
}
```

- [ ] **Step 8: Create ISnapSharpClient.cs**

```csharp
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
```

- [ ] **Step 9: Commit**

```bash
git add src/SnapSharp.Core/Application/Interfaces/
git commit -m "refactor: create Application Interfaces layer with async-only contracts"
```

---

### Task 4: Create Infrastructure layer (Http + Serialization + Token + Messaging)

**Files:**
- Create: `src/SnapSharp.Core/Infrastructure/Http/SnapSharpHttpHandler.cs`
- Create: `src/SnapSharp.Core/Infrastructure/Serialization/SnapSharpErrorResponse.cs`
- Create: `src/SnapSharp.Core/Infrastructure/Serialization/SnapSharpJsonContext.cs`
- Create: `src/SnapSharp.Core/Infrastructure/Token/TokenManager.cs`
- Create: `src/SnapSharp.Core/Infrastructure/Messaging/SnapSharpMessageSender.cs`

**Interfaces:**
- Consumes: `SnapSharp.Domain.*`, `SnapSharp.Application.Interfaces.IAuthService` (for AuthService only), `SnapSharp.Application.Interfaces.ISnapSharpMessageSender` (for SnapSharpMessageSender), `SnapSharp.Application.Contracts.Auth.AccessTokenResponse`
- Produces: `SnapSharp.Infrastructure.Http.SnapSharpHttpHandler`, `SnapSharp.Infrastructure.Token.TokenManager`, `SnapSharp.Infrastructure.Messaging.SnapSharpMessageSender`

- [ ] **Step 1: Create Infrastructure/Http/SnapSharpHttpHandler.cs**

```csharp
using System.Security.Cryptography;
using System.Text;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Domain.Options;

namespace SnapSharp.Infrastructure.Http;

internal sealed class SnapSharpHttpHandler : DelegatingHandler
{
    private readonly SnapSharpOptions _options;
    private readonly RSA _rsa;

    public SnapSharpHttpHandler(SnapSharpOptions options)
        : base(new HttpClientHandler())
    {
        _options = options;
        _rsa = RSA.Create();
        _rsa.ImportFromPem(options.PrivateKeyPem);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
            .ToString("yyyy-MM-ddTHH:mm:sszzz");

        var bodyContent = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;
        var bodyHash = ComputeSha256Hex(bodyContent).ToLowerInvariant();

        var path = request.RequestUri!.PathAndQuery;
        var stringToSign = $"{request.Method}:{path}:{bodyHash}:{timestamp}";

        var signature = ComputeRsaSha256Signature(stringToSign);

        request.Headers.Add("X-TIMESTAMP", timestamp);
        request.Headers.Add("X-SIGNATURE", signature);
        request.Headers.Add("X-CLIENT-KEY", _options.ClientId);
        request.Headers.Add("X-PARTNER-ID", _options.PartnerId);
        request.Headers.Add("CHANNEL-ID", _options.ChannelId);
        request.Headers.Add("X-EXTERNAL-ID", Guid.NewGuid().ToString("N"));

        return await base.SendAsync(request, cancellationToken);
    }

    private static string ComputeSha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }

    private string ComputeRsaSha256Signature(string data)
    {
        try
        {
            var signature = _rsa.SignData(
                Encoding.UTF8.GetBytes(data),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            throw new SnapSharpSignatureException("Failed to compute RSA-SHA256 signature.", ex);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _rsa.Dispose();
        base.Dispose(disposing);
    }
}
```

- [ ] **Step 2: Create Infrastructure/Serialization/SnapSharpErrorResponse.cs**

```csharp
using System.Text.Json.Serialization;

namespace SnapSharp.Infrastructure.Serialization;

internal sealed class SnapSharpErrorResponse
{
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Create Infrastructure/Serialization/SnapSharpJsonContext.cs**

```csharp
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
```

- [ ] **Step 4: Create Infrastructure/Token/TokenManager.cs**

```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Infrastructure.Serialization;

namespace SnapSharp.Infrastructure.Token;

internal sealed class TokenManager : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private AccessTokenResponse? _currentToken;

    public TokenManager(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken ct)
    {
        if (_currentToken is { IsExpired: false })
            return _currentToken.AccessToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_currentToken is { IsExpired: false })
                return _currentToken.AccessToken;

            var body = JsonSerializer.Serialize(
                new { grantType = "client_credentials" },
                SnapSharpJsonContext.Instance.Options);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/access-token/b2b")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new SnapSharpAuthenticationException(
                    ((int)response.StatusCode).ToString(),
                    $"Token request failed: {responseBody}");
            }

            var token = JsonSerializer.Deserialize<AccessTokenResponse>(
                responseBody, SnapSharpJsonContext.Instance.Options);

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new SnapSharpAuthenticationException(
                    token?.ResponseCode ?? "UNKNOWN",
                    $"Access token is empty. {token?.ResponseMessage}");
            }

            _currentToken = token with
            {
                ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 30)
            };

            return _currentToken.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public void Dispose() => _tokenLock.Dispose();
}
```

- [ ] **Step 5: Create Infrastructure/Messaging/SnapSharpMessageSender.cs**

```csharp
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Infrastructure.Serialization;
using SnapSharp.Infrastructure.Token;

namespace SnapSharp.Infrastructure.Messaging;

internal sealed class SnapSharpMessageSender : ISnapSharpMessageSender
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;

    public SnapSharpMessageSender(HttpClient httpClient, TokenManager tokenManager)
    {
        _httpClient = httpClient;
        _tokenManager = tokenManager;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class
    {
        var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, SnapSharpJsonContext.Instance.Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var token = await _tokenManager.GetValidAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var error = JsonSerializer.Deserialize<SnapSharpErrorResponse>(
                    responseBody, SnapSharpJsonContext.Instance.Options);
                if (error is not null)
                    throw new SnapSharpApiException(
                        (int)response.StatusCode, error.ResponseCode, error.ResponseMessage);
            }
            catch (JsonException) { }

            throw new SnapSharpApiException(
                (int)response.StatusCode,
                ((int)response.StatusCode).ToString(),
                responseBody);
        }

        var result = JsonSerializer.Deserialize<TResponse>(
            responseBody, SnapSharpJsonContext.Instance.Options);

        return result ?? throw new SnapSharpException(
            $"Deserialization returned null for {typeof(TResponse).Name}");
    }
}
```

- [ ] **Step 6: Commit**

```bash
git add src/SnapSharp.Core/Infrastructure/
git commit -m "refactor: create Infrastructure layer with HTTP, serialization, token, messaging"
```

---

### Task 5: Create Application Services (async-only, depend on ISnapSharpMessageSender)

**Files:**
- Create: `src/SnapSharp.Core/Application/Services/AuthService.cs`
- Create: `src/SnapSharp.Core/Application/Services/AccountService.cs`
- Create: `src/SnapSharp.Core/Application/Services/TransferService.cs`
- Create: `src/SnapSharp.Core/Application/Services/VirtualAccountService.cs`
- Create: `src/SnapSharp.Core/Application/Services/QrisService.cs`
- Create: `src/SnapSharp.Core/Application/Services/DirectDebitService.cs`

**Interfaces:**
- Consumes: `SnapSharp.Application.Interfaces.ISnapSharpMessageSender`, `SnapSharp.Application.Interfaces.I*Service`, `SnapSharp.Application.Contracts.*`
- Produces: `AuthService`, `AccountService`, `TransferService`, `VirtualAccountService`, `QrisService`, `DirectDebitService`

- [ ] **Step 1: Create Application/Services/AuthService.cs**

```csharp
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;

namespace SnapSharp.Application.Services;

internal sealed class AuthService : IAuthService
{
    private readonly ISnapSharpMessageSender _sender;
    private const string TokenPathB2B = "v1.0/access-token/b2b";
    private const string TokenPathB2B2C = "v1.0/access-token/b2b2c";

    public AuthService(ISnapSharpMessageSender sender)
    {
        _sender = sender;
    }

    public async Task<AccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var body = new { grantType = "client_credentials" };

        var response = await _sender.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B, body, cancellationToken);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new SnapSharpAuthenticationException(
                response.ResponseCode,
                $"Access token is empty. {response.ResponseMessage}");
        }

        return response with
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 30)
        };
    }

    public async Task<AccessTokenResponse> GetAccessTokenB2b2cAsync(
        string customerNo,
        string accountNo,
        Dictionary<string, string>? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        var body = new B2b2cTokenRequest
        {
            CustomerNo = customerNo,
            AccountNo = accountNo,
            AdditionalInfo = additionalInfo
        };

        var response = await _sender.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B2C, body, cancellationToken);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new SnapSharpAuthenticationException(
                response.ResponseCode,
                $"B2B2C access token is empty. {response.ResponseMessage}");
        }

        return response with
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 30)
        };
    }
}
```

- [ ] **Step 2: Create Application/Services/AccountService.cs**

```csharp
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
```

- [ ] **Step 3: Create Application/Services/TransferService.cs**

```csharp
using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class TransferService : ITransferService
{
    private const string CreditTransferPath = "v1.0/transfer-va/credit";
    private const string HistoryPath = "v1.0/transaction-history";
    private readonly ISnapSharpMessageSender _sender;

    public TransferService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<CreditTransferResponse> CreditTransferAsync(
        CreditTransferRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<CreditTransferResponse>(
            HttpMethod.Post, CreditTransferPath, request, ct);
    }

    public async Task<TransactionHistoryResponse> GetHistoryAsync(
        TransactionHistoryRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<TransactionHistoryResponse>(
            HttpMethod.Post, HistoryPath, request, ct);
    }
}
```

- [ ] **Step 4: Create Application/Services/VirtualAccountService.cs**

```csharp
using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class VirtualAccountService : IVirtualAccountService
{
    private const string CreatePath = "v1.0/transfer-va/create-va";
    private const string InquiryPath = "v1.0/transfer-va/inquiry";
    private const string PaymentNotifyPath = "v1.0/transfer-va/payment";
    private readonly ISnapSharpMessageSender _sender;

    public VirtualAccountService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<CreateVaResponse> CreateAsync(
        CreateVaRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<CreateVaResponse>(
            HttpMethod.Post, CreatePath, request, ct);
    }

    public async Task<VAInquiryResponse> InquiryAsync(
        VAInquiryRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<VAInquiryResponse>(
            HttpMethod.Post, InquiryPath, request, ct);
    }

    public async Task<VAPaymentNotifyResponse> PaymentNotifyAsync(
        VAPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<VAPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct);
    }
}
```

- [ ] **Step 5: Create Application/Services/QrisService.cs**

```csharp
using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class QrisService : IQrisService
{
    private const string GeneratePath = "v1.0/qr/qr-generate";
    private const string PaymentNotifyPath = "v1.0/qr/qr-payment";
    private readonly ISnapSharpMessageSender _sender;

    public QrisService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<QrisGenerateResponse> GenerateAsync(
        QrisGenerateRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<QrisGenerateResponse>(
            HttpMethod.Post, GeneratePath, request, ct);
    }

    public async Task<QrisPaymentNotifyResponse> PaymentNotifyAsync(
        QrisPaymentNotifyRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<QrisPaymentNotifyResponse>(
            HttpMethod.Post, PaymentNotifyPath, request, ct);
    }
}
```

- [ ] **Step 6: Create Application/Services/DirectDebitService.cs**

```csharp
using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Interfaces;

namespace SnapSharp.Application.Services;

internal sealed class DirectDebitService : IDirectDebitService
{
    private const string RegisterPath = "v1.0/debit/registration";
    private const string PaymentPath = "v1.0/debit/payment";
    private readonly ISnapSharpMessageSender _sender;

    public DirectDebitService(ISnapSharpMessageSender sender) => _sender = sender;

    public async Task<DirectDebitRegisterResponse> RegisterAsync(
        DirectDebitRegisterRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<DirectDebitRegisterResponse>(
            HttpMethod.Post, RegisterPath, request, ct);
    }

    public async Task<DirectDebitPaymentResponse> PaymentAsync(
        DirectDebitPaymentRequest request, CancellationToken ct = default)
    {
        return await _sender.SendAsync<DirectDebitPaymentResponse>(
            HttpMethod.Post, PaymentPath, request, ct);
    }
}
```

- [ ] **Step 7: Commit**

```bash
git add src/SnapSharp.Core/Application/Services/
git commit -m "refactor: create Application Services with async-only implementations"
```

---

### Task 6: Create sync extensions + Client facade + DI registration

**Files:**
- Create: `src/SnapSharp.Core/Application/Extensions/ServiceSyncExtensions.cs`
- Create: `src/SnapSharp.Core/Client/SnapSharpClient.cs`
- Create: `src/SnapSharp.Core/Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

**Interfaces:**
- Consumes: All service interfaces, `TokenManager`, `SnapSharpMessageSender`, `SnapSharpHttpHandler`
- Produces: `SnapSharpClient` (public facade), `AddSnapSharp()` extension method

- [ ] **Step 1: Create Application/Extensions/ServiceSyncExtensions.cs**

```csharp
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
```

- [ ] **Step 2: Create Client/SnapSharpClient.cs**

```csharp
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
```

- [ ] **Step 3: Create Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs**

```csharp
using System.Net.Http.Headers;
using SnapSharp.Application.Interfaces;
using SnapSharp.Application.Services;
using SnapSharp.Domain.Options;
using SnapSharp.Infrastructure.Http;
using SnapSharp.Infrastructure.Messaging;
using SnapSharp.Infrastructure.Token;

namespace Microsoft.Extensions.DependencyInjection;

public static class SnapSharpServiceCollectionExtensions
{
    public static IServiceCollection AddSnapSharp(
        this IServiceCollection services, SnapSharpOptions options)
    {
        services.AddSingleton(options);

        // TokenManager — own typed HttpClient for direct token endpoint access (avoids circular dep)
        services.AddHttpClient<TokenManager>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler<SnapSharpHttpHandler>();

        // SnapSharpMessageSender — typed HttpClient for business API calls
        services.AddHttpClient<ISnapSharpMessageSender, SnapSharpMessageSender>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<SnapSharpHttpHandler>();

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<ITransferService, TransferService>();
        services.AddSingleton<IVirtualAccountService, VirtualAccountService>();
        services.AddSingleton<IQrisService, QrisService>();
        services.AddSingleton<IDirectDebitService, DirectDebitService>();

        // Client facade
        services.AddSingleton<ISnapSharpClient, SnapSharpClient>();

        return services;
    }
}
```

- [ ] **Step 4: Commit**

```bash
git add src/SnapSharp.Core/Application/Extensions/ src/SnapSharp.Core/Client/ src/SnapSharp.Core/Infrastructure/DependencyInjection/
git commit -m "refactor: create sync extensions, client facade, and DI registration"
```

---

### Task 7: Add package reference for Microsoft.Extensions.Http

**Files:**
- Modify: `src/SnapSharp.Core/SnapSharp.Core.csproj`

- [ ] **Step 1: Update SnapSharp.Core.csproj**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Restore packages**

```bash
dotnet restore src/SnapSharp.Core/SnapSharp.Core.csproj
```

Expected: No errors.

- [ ] **Step 3: Commit**

```bash
git add src/SnapSharp.Core/SnapSharp.Core.csproj
git commit -m "build: add Microsoft.Extensions.Http package reference"
```

---

### Task 8: Delete old files

**Files:**
- Delete: `src/SnapSharp.Core/Models/` (all 12 files)
- Delete: `src/SnapSharp.Core/Authentication/IAuthService.cs`
- Delete: `src/SnapSharp.Core/Authentication/AuthService.cs`
- Delete: `src/SnapSharp.Core/Services/ServiceInterfaces.cs`
- Delete: `src/SnapSharp.Core/Services/AccountService.cs`
- Delete: `src/SnapSharp.Core/Services/TransferService.cs`
- Delete: `src/SnapSharp.Core/Services/VirtualAccountService.cs`
- Delete: `src/SnapSharp.Core/Services/QrisService.cs`
- Delete: `src/SnapSharp.Core/Services/DirectDebitService.cs`
- Delete: `src/SnapSharp.Core/Services/SyncHelper.cs`
- Delete: `src/SnapSharp.Core/Http/SnapSharpHttpHandler.cs`
- Delete: `src/SnapSharp.Core/SnapSharpJsonContext.cs`
- Delete: `src/SnapSharp.Core/SnapSharpClient.cs`
- Delete: `src/SnapSharp.Core/ISnapSharpClient.cs`
- Delete: `src/SnapSharp.Core/SnapSharpOptions.cs`
- Delete: `src/SnapSharp.Core/Exceptions/SnapSharpException.cs`

- [ ] **Step 1: Delete old files**

```bash
rm -r src/SnapSharp.Core/Models/
rm -r src/SnapSharp.Core/Authentication/
rm -r src/SnapSharp.Core/Services/
rm -r src/SnapSharp.Core/Http/
rm -r src/SnapSharp.Core/Exceptions/
rm src/SnapSharp.Core/SnapSharpJsonContext.cs
rm src/SnapSharp.Core/SnapSharpClient.cs
rm src/SnapSharp.Core/ISnapSharpClient.cs
rm src/SnapSharp.Core/SnapSharpOptions.cs
```

- [ ] **Step 2: Verify build fails cleanly (missing types from old namespaces)**

```bash
dotnet build src/SnapSharp.Core/SnapSharp.Core.csproj 2>&1
```

Expected: FAIL — errors about missing old namespace types (not contaminated files). If errors refer to OLD files that still exist, check deletion.

- [ ] **Step 3: Commit**

```bash
git add -u
git commit -m "refactor: delete old files replaced by clean architecture layers"
```

---

### Task 9: Build and fix SnapSharp.Core

- [ ] **Step 1: Build SnapSharp.Core**

```bash
dotnet build src/SnapSharp.Core/SnapSharp.Core.csproj 2>&1
```

Expected: PASS with zero errors.

Fix any compilation errors — likely missing using directives or namespace references. Common issues:
- `SnapSharpOptions` now at `SnapSharp.Domain.Options`
- Exceptions now at `SnapSharp.Domain.Exceptions`
- `Money` now at `SnapSharp.Domain.ValueObjects`
- `ISnapSharpClient` now at `SnapSharp.Application.Interfaces`
- `SnapSharpClient` now at `SnapSharp` namespace (unchanged)

- [ ] **Step 2: If build fails, fix errors and rebuild — repeat until zero errors**

- [ ] **Step 3: Commit any fixes**

```bash
git add -A && git commit -m "fix: resolve compilation errors after clean architecture refactor"
```

---

### Task 10: Update SnapSharp.ReferenceApp

**Files:**
- Modify: `src/SnapSharp.ReferenceApp/Program.cs`
- Modify (if needed): `src/SnapSharp.ReferenceApp/SnapSharp.ReferenceApp.csproj`

- [ ] **Step 1: Update SnapSharp.ReferenceApp/Program.cs**

Replace all old `using` directives with new namespaces. Update DI to use `AddSnapSharp()`.

```csharp
using SnapSharp;
using SnapSharp.Application.Contracts.Account;
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Domain.Options;

var builder = WebApplication.CreateBuilder(args);

// ── SnapSharp Client ──────────────────────────────────────────────────────────
var snapSection = builder.Configuration.GetSection("SnapSharp");
var snapOptions = new SnapSharpOptions
{
    BaseUrl = snapSection["BaseUrl"] ?? "https://sandbox.bank.co.id",
    ClientId = snapSection["ClientId"] ?? "",
    PrivateKeyPem = snapSection["PrivateKeyPem"] ?? "",
    ChannelId = snapSection["ChannelId"] ?? "95221",
    PartnerId = snapSection["PartnerId"] ?? "",
    TimeoutSeconds = int.Parse(snapSection["TimeoutSeconds"] ?? "30"),
    MaxRetries = int.Parse(snapSection["MaxRetries"] ?? "3"),
};

builder.Services.AddSnapSharp(snapOptions);

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SnapSharp Reference API",
        Version = "v1",
        Description = "Reference implementation for BI SNAP integration using SnapSharp SDK"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SnapSharp v1");
        options.RoutePrefix = "swagger";
    });
}

// ── Health Check ─────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
        .ToString("yyyy-MM-ddTHH:mm:sszzz")
}))
.WithTags("Health")
.WithOpenApi();

// ── Auth Endpoints ───────────────────────────────────────────────────────────
var authGroup = app.MapGroup("/auth")
    .WithTags("Authentication");

authGroup.MapPost("/token", async (ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var token = await client.Auth.GetAccessTokenAsync(ct);
        return Results.Ok(token);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Get B2B access token";
    return ops;
});

authGroup.MapPost("/token/b2b2c", async (B2b2cAuthRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var token = await client.Auth.GetAccessTokenB2b2cAsync(
            req.CustomerNo, req.AccountNo, req.AdditionalInfo, ct);
        return Results.Ok(token);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Get B2B2C access token";
    return ops;
});

// ── Account Endpoints ────────────────────────────────────────────────────────
var accountGroup = app.MapGroup("/account")
    .WithTags("Account");

accountGroup.MapPost("/inquiry", async (AccountRegistrationRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Account.InquiryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Account registration inquiry";
    return ops;
});

accountGroup.MapPost("/balance", async (AccountBalanceRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Account.GetBalanceAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Balance inquiry";
    return ops;
});

// ── Transfer Endpoints ───────────────────────────────────────────────────────
var transferGroup = app.MapGroup("/transfer")
    .WithTags("Transfer");

transferGroup.MapPost("/credit", async (CreditTransferRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Transfer.CreditTransferAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Credit transfer (internal & interbank)";
    return ops;
});

transferGroup.MapPost("/history", async (TransactionHistoryRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Transfer.GetHistoryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Transaction history";
    return ops;
});

// ── Virtual Account Endpoints ────────────────────────────────────────────────
var vaGroup = app.MapGroup("/va")
    .WithTags("Virtual Account");

vaGroup.MapPost("/create", async (CreateVaRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.CreateAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Create virtual account";
    return ops;
});

vaGroup.MapPost("/inquiry", async (VAInquiryRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.InquiryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Virtual account inquiry";
    return ops;
});

vaGroup.MapPost("/payment", async (VAPaymentNotifyRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.PaymentNotifyAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Virtual account payment notification";
    return ops;
});

// ── QRIS Endpoints ───────────────────────────────────────────────────────────
var qrisGroup = app.MapGroup("/qris")
    .WithTags("QRIS");

qrisGroup.MapPost("/generate", async (QrisGenerateRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Qris.GenerateAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Generate QRIS";
    return ops;
});

qrisGroup.MapPost("/payment", async (QrisPaymentNotifyRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Qris.PaymentNotifyAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "QRIS payment notification";
    return ops;
});

// ── Direct Debit Endpoints ───────────────────────────────────────────────────
var debitGroup = app.MapGroup("/debit")
    .WithTags("Direct Debit");

debitGroup.MapPost("/register", async (DirectDebitRegisterRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.DirectDebit.RegisterAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Direct debit registration";
    return ops;
});

debitGroup.MapPost("/payment", async (DirectDebitPaymentRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.DirectDebit.PaymentAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Direct debit payment";
    return ops;
});

app.Run();

// ── DTO ──────────────────────────────────────────────────────────────────────
internal sealed record B2b2cAuthRequest(
    string CustomerNo,
    string AccountNo,
    Dictionary<string, string>? AdditionalInfo = null);
```

- [ ] **Step 2: Build ReferenceApp**

```bash
dotnet build src/SnapSharp.ReferenceApp/SnapSharp.ReferenceApp.csproj 2>&1
```

Expected: PASS with zero errors.

- [ ] **Step 3: Commit**

```bash
git add src/SnapSharp.ReferenceApp/Program.cs
git commit -m "refactor: update ReferenceApp to use new clean architecture namespaces and DI"
```

---

### Task 11: Update SnapSharp.Cli (if needed) and final verification

**Files:**
- Modify (if needed): `src/SnapSharp.Cli/Program.cs`

- [ ] **Step 1: Check SnapSharp.Cli uses old namespaces**

```bash
grep -r "using SnapSharp" src/SnapSharp.Cli/Program.cs
```

If it uses old namespaces that changed, update them.

- [ ] **Step 2: Build entire solution**

```bash
dotnet build 2>&1
```

Expected: All 3 projects pass with zero errors.

- [ ] **Step 3: Run tests (if available)**

```bash
dotnet test 2>&1
```

Expected: All tests pass or 0 tests found (no test project with tests yet).

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "refactor: final verification — full solution build passes"
```

---

### Task 12: Add InternalsVisibleTo for test project

**Files:**
- Modify: `src/SnapSharp.Core/SnapSharp.Core.csproj`

- [ ] **Step 1: Add InternalsVisibleTo to .csproj if test project exists**

```xml
<ItemGroup>
  <InternalsVisibleTo Include="SnapSharp.Core.Tests" />
</ItemGroup>
```

(Only if test project `SnapSharp.Core.Tests` exists — check first with `ls test/` or `find . -name "*Test*.csproj"`)

- [ ] **Step 2: Commit**

```bash
git add src/SnapSharp.Core/SnapSharp.Core.csproj && git commit -m "build: add InternalsVisibleTo for test project"
```

---

## Post-Implementation Verification Checklist

- [ ] `dotnet build` — zero errors di semua project
- [ ] `dotnet test` — semua test pass (atau 0 tests found jika belum ada)
- [ ] Tidak ada `using SnapSharp.Models` di file manapun
- [ ] Tidak ada `using SnapSharp.Authentication` di file manapun  
- [ ] Tidak ada `using SnapSharp.Services` di file manapun
- [ ] Tidak ada `using SnapSharp.Exceptions` di file manapun
- [ ] Tidak ada file tersisa di folder lama (`Models/`, `Services/`, `Authentication/`, `Http/`, `Exceptions/`)
- [ ] `SnapSharpClient` namespace tetap `SnapSharp` (backward-compat)
- [ ] `ISnapSharpClient` namespace tetap bisa diakses publik
- [ ] `AddSnapSharp()` extension method tersedia di `Microsoft.Extensions.DependencyInjection`
