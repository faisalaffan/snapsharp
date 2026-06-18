# Changelog

Semua perubahan signifikan pada project ini akan dicatat di file ini.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
Convention: [Semantic Versioning](https://semver.org/spec/v2.0.0.html)

## [Unreleased]

### Added
- B2B Authentication (OAuth2 client credentials + RSA-SHA256 signing)
- B2B2C Authentication (customer-scoped access token)
- Account Registration Inquiry service
- Balance Inquiry service
- Transaction History service
- Credit Transfer service (internal & interbank)
- Direct Debit Registration & Payment services
- Virtual Account (create, inquiry, payment notification) services
- QRIS (generate, payment notification) services
- CLI tool (`snapsharp`) with keygen, validate, sandbox, sign commands
- ASP.NET Core Minimal API reference implementation with Swagger
- Docker support (Dockerfile + docker-compose)
- CI/CD workflows (GitHub Actions)
- Documentation (getting started, authentication, services)
- Zero external dependency — only System.Text.Json

### Planned (v1.0.0)
- Full test coverage (≥ 80% line)
- NuGet publish
- XML doc comments on all public API
- Multi-target: net6.0 + net8.0 support
- Sandbox mock server for offline testing
