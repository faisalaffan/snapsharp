# Credit Transfer

## Endpoint

`POST /v1.0/transfer-va/credit`

## Transfer Internal

```csharp
var request = new CreditTransferRequest
{
    Amount = new Money { Value = "100000.00", Currency = "IDR" },
    SourceAccountNo = "1234567890",
    BeneficiaryAccountNo = "0987654321",
    BeneficiaryName = "Penerima Internal",
    Remark = "Pembayaran invoice #123",
    TransactionType = "internal"
};

var response = await client.Transfer.CreditTransferAsync(request);
```

## Transfer Antar Bank (Interbank)

```csharp
var request = new CreditTransferRequest
{
    Amount = new Money { Value = "250000.00", Currency = "IDR" },
    SourceAccountNo = "1234567890",
    BeneficiaryAccountNo = "1234567800",
    BeneficiaryBankCode = "002", // Kode bank tujuan
    BeneficiaryName = "Penerima Bank Lain",
    Remark = "Transfer antar bank",
    TransactionType = "interbank"
};

var response = await client.Transfer.CreditTransferAsync(request);
```

## Response

```csharp
public sealed class CreditTransferResponse : BaseResponse
{
    public string TransactionId { get; init; }
    public string TransactionDate { get; init; }
    public string SourceAccountNo { get; init; }
    public string BeneficiaryAccountNo { get; init; }
    public Money Amount { get; init; }
    public Money Fee { get; init; }
    public string Status { get; init; }
}

Console.WriteLine($"Transaction ID: {response.TransactionId}");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Fee: {response.Fee.Value} {response.Fee.Currency}");
```
