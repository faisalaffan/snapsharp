# QRIS

## Generate QRIS

```csharp
var request = new QrisGenerateRequest
{
    Amount = new Money { Value = "25000.00" },
    MerchantId = "MERCH001",
    StoreId = "STORE01",
    ValidityPeriod = 300 // 5 menit
};

var response = await client.Qris.GenerateAsync(request);
Console.WriteLine($"QR Data: {response.QrCodeData}");
Console.WriteLine($"QR URL: {response.QrCodeUrl}");
```

## Payment Notification

```csharp
var request = new QrisPaymentNotifyRequest
{
    OriginalPartnerReferenceNo = "REF-001",
    OriginalReferenceNo = "OREF-001",
    TransactionDate = DateTimeOffset.UtcNow,
    Amount = new Money { Value = "25000.00" },
    MerchantId = "MERCH001",
    StoreId = "STORE01",
    TerminalId = "TERM01"
};

var response = await client.Qris.PaymentNotifyAsync(request);
```
