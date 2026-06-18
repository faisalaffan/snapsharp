# Authentication

## B2B Authentication

Authentikasi antar sistem (server-to-server) menggunakan OAuth2 Client Credentials.

### Flow

```
Client                          Server
  |                                |
  |-- POST /v1.0/access-token/b2b  |
  |   {grantType: "client_credentials"} |
  |                                |
  |<-- {accessToken, expiresIn} ---|
```

### Implementasi

```csharp
using var client = new SnapSharpClient(options);
var token = await client.Auth.GetAccessTokenAsync();
// Token auto-refresh: SnapSharp melakukan refresh 30 detik sebelum expiry
```

## B2B2C Authentication

Authentikasi dengan konteks nasabah (customer) — diperlukan untuk transaksi atas nama nasabah.

### Flow

```
Client                                    Server
  |                                          |
  |-- POST /v1.0/access-token/b2b2c          |
  |   {grantType, customerNo, accountNo}     |
  |                                          |
  |<-- {accessToken, expiresIn, additionalInfo}|
```

### Implementasi

```csharp
var token = await client.Auth.GetAccessTokenB2b2cAsync(
    customerNo: "1234567890",
    accountNo: "1234567890",
    additionalInfo: new Dictionary<string, string>
    {
        ["purpose"] = "balance-inquiry"
    });
```

## Request Signing

BI SNAP menggunakan asymmetric signature RSA-SHA256 untuk setiap request.

### Algoritma

```
bodyHash   = SHA256(requestBody) → lowercase hex
stringToSign = "{HTTP_METHOD}:{path}:{bodyHash}:{timestamp}"
signature  = RSA-SHA256-PKCS1(privateKey, stringToSign) → base64
```

### Header Wajib

| Header | Keterangan |
|--------|------------|
| `X-TIMESTAMP` | ISO 8601 timestamp WIB |
| `X-SIGNATURE` | RSA-SHA256 signature (base64) |
| `X-CLIENT-KEY` | Client ID |
| `X-PARTNER-ID` | Partner ID |
| `CHANNEL-ID` | Channel identifier |
| `X-EXTERNAL-ID` | Unique request ID (GUID) |

### Token Management

- Token B2B expiry: 15 menit
- SnapSharp melakukan auto-refresh 30 detik sebelum expiry
- Thread-safe: concurrent request tidak menyebabkan race condition pada token refresh
- `SemaphoreSlim` digunakan untuk double-check locking

## RSA Key Generation

### Menggunakan CLI

```bash
snapsharp keygen -o ./keys
# Output: private-key.pem, public-key.pem
```

### Manual (OpenSSL)

```bash
openssl genpkey -algorithm RSA -out private-key.pem -pkeyopt rsa_keygen_bits:2048
openssl rsa -pubout -in private-key.pem -out public-key.pem
```

### Debug Signature

```bash
snapsharp sign --method POST --path /v1.0/balance-inquiry \
  --body request.json --key private-key.pem
```
