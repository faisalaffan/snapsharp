using SnapSharp;

// Konfigurasi dari environment variable atau inline
var options = new SnapSharpOptions
{
    BaseUrl = Environment.GetEnvironmentVariable("SNAPSHARP_BASE_URL") ?? "https://sandbox.bank.co.id",
    ClientId = Environment.GetEnvironmentVariable("SNAPSHARP_CLIENT_ID") ?? "your-client-id",
    PrivateKeyPem = File.ReadAllText(
        Environment.GetEnvironmentVariable("SNAPSHARP_PRIVATE_KEY_PATH") ?? "private-key.pem"),
    ChannelId = Environment.GetEnvironmentVariable("SNAPSHARP_CHANNEL_ID") ?? "95221",
    PartnerId = Environment.GetEnvironmentVariable("SNAPSHARP_PARTNER_ID") ?? "your-partner-id"
};

using var client = new SnapSharpClient(options);

// Dapatkan access token
var token = await client.Auth.GetAccessTokenAsync();
Console.WriteLine($"Token acquired: {token.AccessToken[..10]}... (expires in {token.ExpiresIn}s)");

// Cek saldo
try
{
    var balance = await client.Account.GetBalanceAsync(new SnapSharp.Models.AccountBalanceRequest
    {
        AccountNo = "1234567890"
    });
    Console.WriteLine($"Balance: {balance.AvailableBalance.Value} {balance.AvailableBalance.Currency}");
}
catch (Exception ex)
{
    Console.WriteLine($"Balance inquiry failed: {ex.Message}");
}
