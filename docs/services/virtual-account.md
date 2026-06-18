# Virtual Account

## Create VA

```csharp
var request = new CreateVaRequest
{
    PartnerServiceId = "12345",
    CustomerNo = "67890",
    VirtualAccountName = "John Doe",
    TotalAmount = new Money { Value = "500000.00" },
    ExpiredDate = DateTimeOffset.UtcNow.AddDays(1),
    BillDetails = new List<VABillDetail>
    {
        new()
        {
            BillCode = "01",
            BillName = "Biaya Kuliah",
            BillAmount = new Money { Value = "500000.00" }
        }
    }
};

var response = await client.VirtualAccount.CreateAsync(request);
Console.WriteLine($"VA Number: {response.VirtualAccountNo}");
Console.WriteLine($"Status: {response.VaStatus}");
```

## VA Inquiry

```csharp
var request = new VAInquiryRequest
{
    PartnerServiceId = "12345",
    CustomerNo = "67890",
    VirtualAccountNo = "1234567890",
    TrxId = "TRX-001"
};

var response = await client.VirtualAccount.InquiryAsync(request);
Console.WriteLine($"VA Name: {response.VirtualAccountName}");
Console.WriteLine($"Total: {response.TotalAmount.Value}");
Console.WriteLine($"Status: {response.VaStatus}");
```

## Payment Notification

```csharp
var request = new VAPaymentNotifyRequest
{
    PartnerServiceId = "12345",
    CustomerNo = "67890",
    VirtualAccountNo = "1234567890",
    TrxId = "TRX-001",
    PaymentRequestId = "PAY-001",
    Amount = new Money { Value = "500000.00" },
    PaidAmount = new Money { Value = "500000.00" }
};

var response = await client.VirtualAccount.PaymentNotifyAsync(request);
```
