# SnapSharp Clean Architecture Refactor — Design Spec

**Date:** 2026-06-18  
**Branch:** dev  
**Author:** Muhammad Faisal Affan  

## Goal

Refactor `SnapSharp.Core` ke clean architecture: modular, reusable, maintainable. Single-project dengan strict namespace separation.

## Current Problems

| Problem | Detail |
|---------|--------|
| SnapSharpClient gemuk | 127 baris — facade + token lifecycle + HTTP + sync wrapper + service instantiation |
| Service coupling ketat | Service di-`new` langsung di constructor SnapSharpClient, tidak bisa DI/test terisolasi |
| Models flat & campur | 12 file di `Models/`, request/response/enum/common bercampur, beberapa file gabung request+response |
| SyncHelper fragile | `Task.Run().GetAwaiter().GetResult()` bisa deadlock |
| SnapSharpJsonContext campur | Error response model + JSON source generator dalam 1 file |
| Interface redundancy | Setiap service interface punya sync + async (sync hanya wrapper `SyncHelper`) |

## Target Structure

```
SnapSharp.Core/
├── Domain/                        SnapSharp.Domain namespace
│   ├── Enums/                     PaymentEnums.cs
│   ├── ValueObjects/              Money.cs
│   ├── Exceptions/                SnapSharpException.cs (4 exception types)
│   └── Options/                   SnapSharpOptions.cs
│
├── Application/                   SnapSharp.Application namespace
│   ├── Contracts/
│   │   ├── Common/                BaseRequest.cs, BaseResponse.cs
│   │   ├── Auth/                  AccessTokenResponse.cs, B2b2cTokenRequest.cs
│   │   ├── Account/               AccountBalanceRequest.cs + Response, AccountRegistrationRequest.cs + Response
│   │   ├── Transfer/              CreditTransferRequest.cs + Response, TransactionHistoryRequest.cs + Response, TransactionEntry.cs
│   │   ├── VirtualAccount/        CreateVaRequest.cs + VABillDetail + Response, VAInquiryRequest.cs + Response, VAPaymentNotifyRequest.cs + VAPaidBill + Response
│   │   ├── Qris/                  QrisGenerateRequest.cs + Response, QrisPaymentNotifyRequest.cs + Response
│   │   └── DirectDebit/           DirectDebitRegisterRequest.cs + Response, DirectDebitPaymentRequest.cs + Response
│   ├── Interfaces/
│   │   ├── ISnapSharpMessageSender.cs   ← NEW abstraction
│   │   ├── IAuthService.cs
│   │   ├── IAccountService.cs
│   │   ├── ITransferService.cs
│   │   ├── IVirtualAccountService.cs
│   │   ├── IQrisService.cs
│   │   ├── IDirectDebitService.cs
│   │   └── ISnapSharpClient.cs
│   ├── Services/                  Implementations (async-only)
│   │   ├── AuthService.cs
│   │   ├── AccountService.cs
│   │   ├── TransferService.cs
│   │   ├── VirtualAccountService.cs
│   │   ├── QrisService.cs
│   │   └── DirectDebitService.cs
│   └── Extensions/
│       └── ServiceSyncExtensions.cs     ← sync wrappers moved here
│
├── Infrastructure/                SnapSharp.Infrastructure namespace
│   ├── Http/
│   │   └── SnapSharpHttpHandler.cs      (unchanged logic, namespace only)
│   ├── Serialization/
│   │   ├── SnapSharpJsonContext.cs      (JSON source generator only)
│   │   └── SnapSharpErrorResponse.cs    (separated from context)
│   ├── Token/
│   │   └── TokenManager.cs              ← extracted from SnapSharpClient
│   ├── Messaging/
│   │   └── SnapSharpMessageSender.cs    ← implements ISnapSharpMessageSender
│   └── DependencyInjection/
│       └── ServiceCollectionExtensions.cs  ← AddSnapSharp()
│
└── Client/                        SnapSharp namespace (backward-compat)
    └── SnapSharpClient.cs         Facade only (~40 lines)
```

## Dependency Graph

```
Client ─────► Application.Interfaces, Infrastructure (DI + disposal)
Infrastructure.Messaging ──► Application.Interfaces, Infrastructure.Token, Domain
Infrastructure.Token ──► Domain (via own signed HttpClient, no IAuthService)
Application.Services ──► Application.Interfaces, Application.Contracts, Domain
Domain ──► nothing
```

## Key Design Decisions

### 1. ISnapSharpMessageSender abstraction

Service tidak depend ke `SnapSharpClient` concrete, tapi ke `ISnapSharpMessageSender`:

```csharp
public interface ISnapSharpMessageSender
{
    Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class;
}
```

Service constructor: `AccountService(ISnapSharpMessageSender sender)`.  
Unit test: mock `ISnapSharpMessageSender`, verify service call path + body.

### 2. TokenManager — single responsibility, no circular dep

Extract dari `SnapSharpClient.GetValidAccessTokenAsync()`:
- Cache `AccessTokenResponse` dengan `ExpiresAt`
- Double-check locking via `SemaphoreSlim`
- Auto-refresh saat expired

**Circular dependency avoidance:** TokenManager TIDAK boleh depend ke `IAuthService` karena:
`AuthService` → `ISnapSharpMessageSender` → `TokenManager` → `IAuthService` (cycle).

Solusi: TokenManager punya **HttpClient sendiri** (dengan `SnapSharpHttpHandler` untuk signing) dan panggil token endpoint `v1.0/access-token/b2b` langsung — bypass `ISnapSharpMessageSender` dan `IAuthService`.

```
TokenManager ──► HttpClient (sendiri, signed) ──► POST v1.0/access-token/b2b
AuthService  ──► ISnapSharpMessageSender ──► TokenManager (dapat token) ──► kirim request
```

Dependency chain: `TokenManager` → `HttpClient` + `Domain`. Tidak ada cycle.

Token disimpan via `ExpiresAt` (UTC, minus 30s buffer dari `ExpiresIn`).

### 3. Async-only interfaces

Semua sync methods (`GetBalance`, `CreditTransfer`, dll) dihapus dari interface.  
Sync wrapper disediakan via extension method `ServiceSyncExtensions` untuk backward compat.

### 4. Token Authorization header

Sebelumnya: `SnapSharpHttpHandler` inject token + signing headers.  
Sesudah: `SnapSharpHttpHandler` hanya urus signing headers (X-SIGNATURE, X-TIMESTAMP, X-CLIENT-KEY, X-PARTNER-ID, CHANNEL-ID, X-EXTERNAL-ID). Token di-inject oleh `SnapSharpMessageSender` via `Authorization: Bearer {token}`.

### 5. DI Registration

```csharp
services.AddSnapSharp(snapOptions);
```

Registration detail:
- `TokenManager` — typed `HttpClient` sendiri via `AddHttpClient<TokenManager>(...)` dengan `SnapSharpHttpHandler` (untuk call token endpoint, tidak lewat `ISnapSharpMessageSender`)
- `ISnapSharpMessageSender` → `SnapSharpMessageSender` — typed `HttpClient` via `AddHttpClient<ISnapSharpMessageSender, SnapSharpMessageSender>(...)` dengan `SnapSharpHttpHandler`
- `IAuthService` → `AuthService` (depend on `ISnapSharpMessageSender`)
- `IAccountService` → `AccountService` (depend on `ISnapSharpMessageSender`)
- `ITransferService` → `TransferService` (depend on `ISnapSharpMessageSender`)
- `IVirtualAccountService` → `VirtualAccountService` (depend on `ISnapSharpMessageSender`)
- `IQrisService` → `QrisService` (depend on `ISnapSharpMessageSender`)
- `IDirectDebitService` → `DirectDebitService` (depend on `ISnapSharpMessageSender`)
- `ISnapSharpClient` → `SnapSharpClient` (facade, depend on all service interfaces + `HttpClient` + `TokenManager`)

Semua singleton. `HttpClient` lifecycle diatur oleh `IHttpClientFactory`.

### 6. Contracts sub-folder per BI SNAP service

BI SNAP punya service boundary: Auth, Account, Transfer, Virtual Account, QRIS, Direct Debit. Masing-masing jadi sub-folder di `Application/Contracts/`.

### 7. Public API backward compatibility

| Usage | Status |
|-------|--------|
| `using SnapSharp;` | Tidak berubah |
| `ISnapSharpClient client` | Tidak berubah |
| `client.Account.GetBalanceAsync(req, ct)` | Tidak berubah |
| `new SnapSharpOptions { ... }` | Tidak berubah |
| `new SnapSharpClient(options)` | **Berubah** — pakai DI `AddSnapSharp()` |
| Sync methods via extension | Tersedia di `SnapSharp.Application.Extensions` |

### 8. File deletion

File yang dihapus:
- `Services/SyncHelper.cs` — diganti extension method
- Semua `ConfigureAwait(false)` — dihapus (tidak diperlukan di .NET 8 SDK library)

## Non-Goals

- Tidak menambah fitur baru
- Tidak mengubah behavior API calls
- Tidak mengubah target framework (tetap net8.0)
- Tidak menyentuh `SnapSharp.Cli` dan `SnapSharp.ReferenceApp` kecuali update `using` namespace
- Tidak menambah retry/polly/circuit breaker (out of scope)

## Verification

- `dotnet build` harus pass zero errors
- `dotnet test` (jika ada test project) harus pass
- `SnapSharp.ReferenceApp` harus compile dan run
- Tidak ada `SnapSharpException`, `SnapSharpAuthenticationException`, dll yang berubah namespace publiknya (tetap bisa di-catch dengan `using SnapSharp.Domain;`)

## Future Improvements (out of scope)

- Retry policy via Polly
- Circuit breaker
- Logging abstraction (ILogger)
- Multi-target framework (netstandard2.0 + net8.0)
