# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 1.x.x   | ✅ |
| 0.x.x   | ✅ (pre-release) |

## Reporting a Vulnerability

Jangan laporkan vulnerability via public issue. Kirim email ke:

**faisalaffan@proton.me**

Response time: 2-3 hari kerja.

## Scope

SnapSharp menangani cryptographic signing untuk transaksi BI SNAP. Vulnerability yang relevan:

- Signature bypass atau verification flaw pada RSA-SHA256
- Token leakage melalui logging atau exception message
- Private key exposure via memory atau file system
- Replay attack via timestamp/signature validation
- Man-in-the-middle pada komunikasi ke sandbox/production

## Disclosure Policy

- 90 hari untuk perbaikan setelah konfirmasi
- Credit akan diberikan di changelog release
- CVE request jika severity ≥ High
