# Contributing

## Cara Kontribusi

1. **Fork** repository
2. Buat branch: `feat/nama-fitur` atau `fix/nama-bug`
3. Commit dengan [Conventional Commits](https://www.conventionalcommits.org/): `feat:`, `fix:`, `docs:`, `refactor:`, `chore:`
4. Push dan buat Pull Request ke `dev`

## Development Setup

```bash
git clone https://github.com/faisalaffan/snapsharp.git
cd snapsharp
dotnet restore
dotnet build
dotnet test
```

## Konvensi Kode

- Ikuti pattern existing di project
- **Nullable enabled** di semua project — jangan disable
- **File-scoped namespace** (`namespace SnapSharp.Models;`)
- **Sealed class** untuk semua model dan service
- **required init-only properties** untuk model DTO
- **System.Text.Json source generator** untuk serialization
- Async-first: semua public method async, sync wrapper opsional

## BI SNAP Specification

Sebelum kontribusi ke service category baru, pastikan merujuk spesifikasi BI SNAP:

- [API Developer Portal BI](https://apidevportal.bi.go.id/snap/api-services)
- [ASPI Documentation](https://apidevportal.aspi-indonesia.or.id)

## Test

- Unit test dengan **xUnit**
- Target coverage: ≥ 80% line
- Mock HTTP responses dengan `HttpMessageHandler` custom (lihat test existing)
- Test signature generation dengan known vector

## PR Checklist

- [ ] Build: `dotnet build` tanpa error
- [ ] Test: `dotnet test` tanpa failure
- [ ] Format: ikuti konvensi existing
- [ ] Jangan commit perubahan format/lint bareng logic
