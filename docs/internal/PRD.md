# Product Requirements Document
# SnapSharp — .NET SDK for BI SNAP (Standar Nasional Open API Pembayaran)

**Version:** 0.1.0-draft  
**Author:** Muhammad Faisal Affan  
**Status:** Draft  
**Last Updated:** 2026-06-18

---

## 1. Overview

### 1.1 Summary

SnapSharp adalah open-source .NET SDK untuk mengintegrasikan aplikasi dengan standar BI SNAP (Standar Nasional Open API Pembayaran) yang ditetapkan oleh Bank Indonesia. SDK ini menyediakan abstraksi yang bersih, type-safe, dan production-ready untuk seluruh service category BI SNAP, dilengkapi dengan CLI tool untuk sandbox testing dan reference implementation sebagai panduan integrasi.

### 1.2 Problem Statement

BI SNAP telah menjadi standar wajib untuk seluruh payment service provider di Indonesia sejak Juni 2024. Ekosistem Go dan Java sudah memiliki library yang cukup matang untuk integrasi ini, namun ekosistem .NET belum memiliki SDK yang:

- Mengikuti spesifikasi BI SNAP secara lengkap dan benar
- Dipublish secara resmi di NuGet dengan dokumentasi yang memadai
- Zero external dependency (selain `System.Text.Json` yang sudah built-in)
- Mendukung .NET 6/8 dengan async-first pattern
- Menyertakan CLI tool untuk developer experience yang baik

Gap ini menyebabkan developer .NET di Indonesia harus menulis integrasi dari nol, rawan error pada implementasi security signature, dan tidak ada standar komunitas yang bisa dirujuk.

### 1.3 Opportunity

Dengan mandatory compliance BI SNAP yang sudah berjalan, setiap bank dan fintech Indonesia yang memiliki sistem .NET (termasuk BTN, beberapa BPD, dan vendor perbankan) membutuhkan solusi ini. SnapSharp mengisi gap tersebut sebagai first-mover di ekosistem .NET.

---

## 2. Goals & Non-Goals

### 2.1 Goals

- Menyediakan SDK .NET yang fully compliant dengan spesifikasi BI SNAP
- Mendukung seluruh service category BI SNAP: Administrasi, Keamanan, Registrasi, Informasi Saldo, Riwayat Transaksi, Transfer Kredit, Transfer Debit
- Publish ke NuGet dengan target 1.000+ downloads dalam 6 bulan pertama
- Menyertakan CLI tool (`snapsharp`) untuk sandbox testing tanpa perlu menulis kode
- Menyertakan reference implementation (ASP.NET Core minimal API) sebagai contoh integrasi end-to-end
- Zero external dependency: hanya `System.Text.Json` (built-in .NET 6+)
- Support .NET 6 LTS dan .NET 8 LTS

### 2.2 Non-Goals

- Tidak mendukung payment gateway pihak ketiga (Midtrans, Xendit, dll) — fokus pada BI SNAP standard saja
- Tidak menyediakan UI dashboard
- Tidak menyimpan state transaksi (stateless SDK)
- Tidak mendukung .NET Framework (4.x) atau .NET Standard 2.0
- Tidak mengabstraksi logika bisnis bank (validasi saldo, limit, dll)

---

## 3. Target Users

### 3.1 Primary Users

**Developer .NET di perusahaan fintech / bank Indonesia**
- Konteks: Mengintegrasikan sistem internal ke ekosistem BI SNAP
- Pain point: Tidak ada SDK .NET yang reliable, harus implement manual termasuk asymmetric signature
- Ekspektasi: Drop-in library dengan dokumentasi yang jelas

**System integrator / vendor perbankan**
- Konteks: Membangun middleware atau channel banking untuk klien bank
- Pain point: Waktu development yang lama untuk security layer BI SNAP
- Ekspektasi: Production-ready dengan unit test coverage yang tinggi

### 3.2 Secondary Users

**Developer yang sedang belajar BI SNAP**
- Konteks: Ingin memahami flow BI SNAP tanpa membaca spesifikasi 200+ halaman
- Pain point: Dokumentasi BI SNAP teknis dan kurang ada contoh kode
- Ekspektasi: CLI tool dan reference implementation yang bisa dijalankan langsung

---

## 4. Technical Specification

### 4.1 Architecture

```
SnapSharp (Solution)
├── SnapSharp.Core          # SDK utama — NuGet package
│   ├── Authentication/     # OAuth2 + asymmetric signature
│   ├── Services/           # Per-category service clients
│   ├── Models/             # Request/response DTOs
│   ├── Http/               # HttpClient wrapper, retry, logging
│   └── Exceptions/         # SnapSharp-specific exceptions
├── SnapSharp.Cli            # CLI tool — dotnet tool
│   └── Commands/           # sandbox, validate, generate-key, etc.
└── SnapSharp.ReferenceApp   # Reference implementation
    └── (ASP.NET Core Minimal API)
```

### 4.2 BI SNAP Service Coverage

| Service Category | Endpoint | Priority |
|-----------------|----------|----------|
| Keamanan | Access Token B2B | P0 |
| Keamanan | Access Token B2B2C | P0 |
| Administrasi | Account Registration Inquiry | P1 |
| Informasi Saldo | Balance Inquiry | P1 |
| Riwayat Transaksi | Transaction History | P1 |
| Transfer Kredit | Internal Transfer | P0 |
| Transfer Kredit | Transfer to Other Bank | P0 |
| Transfer Debit | Direct Debit Registration | P1 |
| Transfer Debit | Direct Debit Payment | P1 |
| Virtual Account | VA Registration | P0 |
| Virtual Account | VA Payment Notification | P0 |
| QRIS | QRIS Generate | P1 |
| QRIS | QRIS Payment | P1 |

### 4.3 Security Implementation

BI SNAP menggunakan asymmetric signature dengan spesifikasi berikut:

- **Algoritma:** RSA-SHA256 untuk request signing
- **Token:** OAuth2 Bearer Token (B2B) dengan expiry 15 menit
- **Header wajib:** `X-TIMESTAMP`, `X-SIGNATURE`, `X-CLIENT-KEY`, `X-PARTNER-ID`, `CHANNEL-ID`, `X-EXTERNAL-ID`
- **Body hashing:** SHA-256 lowercase hex dari request body sebelum signing

SnapSharp menangani seluruh security layer ini secara transparan — developer hanya perlu menyediakan private key dan client credential.

### 4.4 SDK API Design

```csharp
// Inisialisasi client
var client = new SnapSharpClient(new SnapSharpOptions
{
    BaseUrl = "https://sandbox.bank.co.id",
    ClientId = "your-client-id",
    PrivateKeyPem = File.ReadAllText("private-key.pem"),
    ChannelId = "95221",
    PartnerId = "your-partner-id"
});

// Async (default)
var token = await client.Auth.GetAccessTokenAsync();

// Informasi saldo
var balance = await client.Account.GetBalanceAsync(new BalanceInquiryRequest
{
    AccountNo = "1234567890",
    PartnerReferenceNo = Guid.NewGuid().ToString()
});

// Transfer kredit
var transfer = await client.Transfer.CreditTransferAsync(new CreditTransferRequest
{
    Amount = new Money { Value = "100000.00", Currency = "IDR" },
    BeneficiaryAccountNo = "0987654321",
    PartnerReferenceNo = Guid.NewGuid().ToString()
});

// Sync wrapper (untuk legacy code)
var balanceSync = client.Account.GetBalance(new BalanceInquiryRequest { ... });
```

### 4.5 CLI Tool Design

```bash
# Generate RSA key pair
snapsharp keygen --output ./keys

# Validate konfigurasi dan koneksi ke sandbox
snapsharp validate --config snapsharp.json

# Test endpoint spesifik
snapsharp sandbox balance --account 1234567890
snapsharp sandbox transfer --to 0987654321 --amount 10000

# Generate request signature (untuk debugging manual)
snapsharp sign --method POST --path /v1.0/transfer-va/payment \
  --body request.json --key private-key.pem
```

### 4.6 Dependencies

| Dependency | Justification | Version |
|-----------|--------------|---------|
| `System.Text.Json` | Built-in .NET 6+, zero external dep | built-in |
| `System.Net.Http` | Built-in .NET 6+, HttpClient | built-in |
| `System.Security.Cryptography` | Built-in, RSA signing | built-in |

Zero external NuGet dependency untuk `SnapSharp.Core`. CLI boleh menggunakan `System.CommandLine`.

---

## 5. Functional Requirements

### FR-01: Authentication
- SDK harus mengimplementasikan OAuth2 B2B token acquisition sesuai spesifikasi BI SNAP
- SDK harus melakukan token refresh otomatis sebelum expiry
- SDK harus mengimplementasikan request signing dengan RSA-SHA256

### FR-02: Service Clients
- Setiap service category harus memiliki strongly-typed client class
- Semua request/response harus di-map ke C# model dengan nullable annotation
- Error response BI SNAP harus di-throw sebagai `SnapSharpException` dengan `ResponseCode` dan `ResponseMessage`

### FR-03: Async/Sync Support
- Semua public method harus menyediakan async variant (`*Async`) sebagai primary
- Sync wrapper (`*`) harus tersedia untuk kompatibilitas dengan legacy codebase
- Tidak boleh menggunakan `.Result` atau `.Wait()` di dalam implementasi SDK (deadlock risk)

### FR-04: CLI Tool
- CLI harus bisa diinstall via `dotnet tool install -g SnapSharp.Cli`
- CLI harus menyediakan command `keygen`, `validate`, `sandbox`, dan `sign`
- CLI harus membaca konfigurasi dari `snapsharp.json` atau environment variable

### FR-05: Reference Implementation
- Reference app harus bisa dijalankan dengan `docker compose up`
- Reference app harus mendemonstrasikan minimal 5 service category
- Reference app harus menyertakan Swagger UI

---

## 6. Non-Functional Requirements

| NFR | Target |
|-----|--------|
| Unit test coverage | ≥ 80% line coverage |
| Package size | < 500KB |
| Cold start latency | < 100ms (instansiasi client) |
| .NET target | net6.0; net8.0 |
| Nullable annotations | Enabled di seluruh project |
| CI/CD | GitHub Actions: build, test, publish NuGet |
| Documentation | XML doc comments di semua public API |

---

## 7. Project Structure & Repository

```
snapsharp/
├── src/
│   ├── SnapSharp.Core/
│   ├── SnapSharp.Cli/
│   └── SnapSharp.ReferenceApp/
├── tests/
│   ├── SnapSharp.Core.Tests/
│   └── SnapSharp.Cli.Tests/
├── docs/
│   ├── getting-started.md
│   ├── authentication.md
│   └── services/
├── examples/
│   └── minimal-integration/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── publish.yml
├── README.md
└── snapsharp.sln
```

---

## 8. Milestones & Roadmap

### v0.1.0 — Foundation (Week 1-2)
- Project setup, CI/CD pipeline
- Authentication module (B2B token + RSA signing)
- SnapSharpClient base class dengan HttpClient factory
- Unit test framework setup

### v0.2.0 — Core Services (Week 3-4)
- Balance Inquiry
- Transaction History
- Internal Transfer
- Transfer to Other Bank
- Unit tests untuk semua service

### v0.3.0 — Extended Services (Week 5-6)
- Virtual Account (create, inquiry, payment notification)
- QRIS (generate, payment)
- Direct Debit (register, payment)

### v0.4.0 — CLI & Reference App (Week 7-8)
- CLI tool (`snapsharp`) dengan semua command
- Reference implementation (ASP.NET Core Minimal API)
- Docker Compose setup
- Swagger UI

### v1.0.0 — Production Release
- Full documentation
- NuGet publish
- GitHub release notes
- README yang komprehensif dengan badge

---

## 9. Success Metrics

| Metric | Target (6 bulan) |
|--------|-----------------|
| NuGet downloads | ≥ 1.000 |
| GitHub stars | ≥ 100 |
| Issues/PR dari komunitas | ≥ 5 |
| Covered BI SNAP services | 100% dari P0, ≥ 80% P1 |

---

## 10. Risks & Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|-----------|
| Spesifikasi BI SNAP berubah | Medium | High | Versioning ketat, changelog, ikuti update ASPI |
| Akses sandbox terbatas | High | Medium | Mock server untuk unit test, tidak wajib sandbox aktif untuk development |
| Zero adoption karena niche | Medium | High | Blog post, posting di komunitas .NET Indonesia, reach ke developer bank |
| Security flaw di signing implementation | Low | Critical | Review berdasarkan BI SNAP spec resmi, unit test edge case |

---

## 11. References

- BI SNAP Developer Portal: https://apidevportal.bi.go.id/snap/api-services
- ASPI SNAP Documentation: https://apidevportal.aspi-indonesia.or.id
- BI Governor Decree No. 23/10/KEP.GBI/2021
- Comparable Go implementation: https://github.com/imrenagi/go-payment (Midtrans/Xendit, bukan BI SNAP — untuk referensi pattern)
- BRIAPI SNAP implementation: https://developers.bri.co.id/en/snap-bi

---

*Document ini adalah living document. Update sesuai feedback dari komunitas dan perubahan spesifikasi BI SNAP.*
