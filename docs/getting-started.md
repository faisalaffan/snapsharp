# Getting Started

Panduan cepat integrasi SnapSharp ke aplikasi .NET Anda.

## Prerequisites

- .NET 8 SDK
- Private key RSA 2048-bit (PEM format)
- Sandbox credentials dari bank penyedia BI SNAP

## Instalasi

```bash
dotnet add package SnapSharp.Core
```

## Konfigurasi Dasar

```csharp
using SnapSharp;

var options = new SnapSharpOptions
{
    BaseUrl = "https://sandbox.bank.co.id",
    ClientId = "your-client-id",
    PrivateKeyPem = File.ReadAllText("private-key.pem"),
    ChannelId = "95221",
    PartnerId = "your-partner-id"
};

using var client = new SnapSharpClient(options);
```

## Operasi Pertama: Access Token

```csharp
var token = await client.Auth.GetAccessTokenAsync();
Console.WriteLine($"Token: {token.AccessToken[..10]}...");
Console.WriteLine($"Expires in: {token.ExpiresIn}s");
```

## Operasi Berikutnya

Setelah client terinisialisasi, semua service siap digunakan:

```csharp
// Cek saldo
var balance = await client.Account.GetBalanceAsync(new AccountBalanceRequest
{
    AccountNo = "1234567890"
});

// Transfer
var transfer = await client.Transfer.CreditTransferAsync(new CreditTransferRequest
{
    Amount = new Money { Value = "100000.00", Currency = "IDR" },
    SourceAccountNo = "1234567890",
    BeneficiaryAccountNo = "0987654321",
    BeneficiaryBankCode = "002",
    BeneficiaryName = "Penerima",
    TransactionType = "interbank"
});

// Virtual Account
var va = await client.VirtualAccount.CreateAsync(new CreateVaRequest
{
    PartnerServiceId = "12345",
    CustomerNo = "67890",
    VirtualAccountName = "John Doe",
    TotalAmount = new Money { Value = "500000.00" },
    ExpiredDate = DateTimeOffset.UtcNow.AddDays(1)
});
```

## CLI Tool

```bash
# Install
dotnet tool install -g SnapSharp.Cli

# Generate key
snapsharp keygen -o ./keys

# Test endpoint
snapsharp sandbox balance -a 1234567890 -c snapsharp.json
```

## Reference App

```bash
docker compose up
# Buka http://localhost:8080/swagger
```
