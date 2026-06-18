# Balance Inquiry

## Endpoint

`POST /v1.0/balance-inquiry`

## Request

```csharp
var request = new AccountBalanceRequest
{
    AccountNo = "1234567890",
    BalanceType = "AVAILABLE"
};

var response = await client.Account.GetBalanceAsync(request);
```

## Response

```csharp
public sealed class AccountBalanceResponse : BaseResponse
{
    public string AccountNo { get; init; }
    public Money AvailableBalance { get; init; }
    public Money LedgerBalance { get; init; }
    public Money HoldAmount { get; init; }
    public string AccountStatus { get; init; }
}

// Usage
Console.WriteLine($"Saldo tersedia: {response.AvailableBalance.Value} {response.AvailableBalance.Currency}");
Console.WriteLine($"Saldo ledger:   {response.LedgerBalance.Value} {response.LedgerBalance.Currency}");
```

## Sync Wrapper

```csharp
var response = client.Account.GetBalance(request);
```
