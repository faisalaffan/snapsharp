using System.CommandLine;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// ─── Root Command ───────────────────────────────────────────────────────────
var rootCmd = new RootCommand("SnapSharp CLI — Tool for SnapSharp .NET SDK")
{
    Name = "snapsharp",
};

// ─── keygen ─────────────────────────────────────────────────────────────────
var outputOpt = new Option<DirectoryInfo>(
    ["--output", "-o"],
    () => new DirectoryInfo("./keys"),
    "Output directory for generated keys");

var keygenCmd = new Command("keygen", "Generate RSA 2048-bit key pair")
{
    outputOpt,
};
keygenCmd.SetHandler(KeygenHandler, outputOpt);
rootCmd.AddCommand(keygenCmd);

// ─── validate ───────────────────────────────────────────────────────────────
var configOpt = new Option<FileInfo>(
    ["--config", "-c"],
    () => new FileInfo("snapsharp.json"),
    "Path to config file");

var validateCmd = new Command("validate", "Validate config file")
{
    configOpt,
};
validateCmd.SetHandler(ValidateHandler, configOpt);
rootCmd.AddCommand(validateCmd);

// ─── sandbox (subcommand group) ─────────────────────────────────────────────
var sandboxCmd = new Command("sandbox", "Sandbox API operations");
rootCmd.AddCommand(sandboxCmd);

// sandbox token
var sandboxTokenCmd = new Command("token", "Get access token");
sandboxTokenCmd.AddOption(configOpt);
sandboxTokenCmd.SetHandler(SandboxTokenHandler, configOpt);
sandboxCmd.AddCommand(sandboxTokenCmd);

// sandbox balance
var accountOpt = new Option<string>(["--account", "-a"], "Account number") { IsRequired = true };
var sandboxBalanceCmd = new Command("balance", "Balance inquiry")
{
    accountOpt, configOpt,
};
sandboxBalanceCmd.SetHandler(SandboxBalanceHandler, accountOpt, configOpt);
sandboxCmd.AddCommand(sandboxBalanceCmd);

// sandbox transfer
var transferOpts = new Dictionary<string, Option<string>>
{
    ["to"] = new Option<string>(["--to"], "Beneficiary account number") { IsRequired = true },
    ["amount"] = new Option<string>(["--amount"], "Amount in IDR") { IsRequired = true },
    ["bank"] = new Option<string>(["--bank"], "Beneficiary bank code"),
    ["name"] = new Option<string>(["--name"], "Beneficiary name") { IsRequired = true },
    ["remark"] = new Option<string>(["--remark"], "Transaction remark"),
    ["source"] = new Option<string>(["--source"], "Source account"),
};

var sandboxTransferCmd = new Command("transfer", "Credit transfer");
foreach (var opt in transferOpts.Values)
    sandboxTransferCmd.AddOption(opt);
sandboxTransferCmd.AddOption(configOpt);
sandboxTransferCmd.SetHandler(
    (to, amount, bank, name, remark, source, config) =>
        SandboxTransferHandler(to!, amount!, bank, name!, remark, source, config!),
    transferOpts["to"]!,
    transferOpts["amount"]!,
    transferOpts["bank"]!,
    transferOpts["name"]!,
    transferOpts["remark"]!,
    transferOpts["source"]!,
    configOpt);
sandboxCmd.AddCommand(sandboxTransferCmd);

// sandbox va-create
var vaCreateOpts = new Dictionary<string, Option<string>>
{
    ["partnerServiceId"] = new Option<string>(["--partner-service-id"], "Partner service ID") { IsRequired = true },
    ["customerNo"] = new Option<string>(["--customer-no"], "Customer number") { IsRequired = true },
    ["name"] = new Option<string>(["--name"], "VA name") { IsRequired = true },
    ["amount"] = new Option<string>(["--amount"], "Amount") { IsRequired = true },
    ["expired"] = new Option<string>(["--expired"], "Expiration date (ISO format)") { IsRequired = true },
};

var sandboxVaCreateCmd = new Command("va-create", "Create Virtual Account");
foreach (var opt in vaCreateOpts.Values)
    sandboxVaCreateCmd.AddOption(opt);
sandboxVaCreateCmd.AddOption(configOpt);
sandboxVaCreateCmd.SetHandler(
    (partnerServiceId, customerNo, name, amount, expired, config) =>
        SandboxVaCreateHandler(partnerServiceId!, customerNo!, name!, amount!, expired!, config!),
    vaCreateOpts["partnerServiceId"]!,
    vaCreateOpts["customerNo"]!,
    vaCreateOpts["name"]!,
    vaCreateOpts["amount"]!,
    vaCreateOpts["expired"]!,
    configOpt);
sandboxCmd.AddCommand(sandboxVaCreateCmd);

// sandbox qris
var qrisOpts = new Dictionary<string, Option<string>>
{
    ["amount"] = new Option<string>(["--amount"], "Amount") { IsRequired = true },
    ["merchantId"] = new Option<string>(["--merchant-id"], "Merchant ID") { IsRequired = true },
    ["storeId"] = new Option<string>(["--store-id"], "Store ID") { IsRequired = true },
};

var sandboxQrisCmd = new Command("qris", "Generate QRIS");
foreach (var opt in qrisOpts.Values)
    sandboxQrisCmd.AddOption(opt);
sandboxQrisCmd.AddOption(configOpt);
sandboxQrisCmd.SetHandler(
    (amount, merchantId, storeId, config) =>
        SandboxQrisHandler(amount!, merchantId!, storeId!, config!),
    qrisOpts["amount"]!,
    qrisOpts["merchantId"]!,
    qrisOpts["storeId"]!,
    configOpt);
sandboxCmd.AddCommand(sandboxQrisCmd);

// ─── sign ───────────────────────────────────────────────────────────────────
var methodOpt = new Option<string>(["--method"], "HTTP method (e.g. POST)") { IsRequired = true };
var pathOpt = new Option<string>(["--path"], "URL path (e.g. /v1.0/balance-inquiry)") { IsRequired = true };
var bodyOpt = new Option<FileInfo?>(["--body"], "Path to JSON request body file");
var keyOpt = new Option<FileInfo>(["--key"], "Path to private key PEM file") { IsRequired = true };
var timestampOpt = new Option<string?>(["--timestamp"], "ISO timestamp (auto if omitted)");

var signCmd = new Command("sign", "Generate request signature (debugging)")
{
    methodOpt, pathOpt, bodyOpt, keyOpt, timestampOpt,
};
signCmd.SetHandler(SignHandler, methodOpt, pathOpt, bodyOpt, keyOpt, timestampOpt);
rootCmd.AddCommand(signCmd);

// ─── Entry Point ────────────────────────────────────────────────────────────
return await rootCmd.InvokeAsync(args);


// ═══════════════════════════════════════════════════════════════════════════
//  Handlers
// ═══════════════════════════════════════════════════════════════════════════

static void KeygenHandler(DirectoryInfo output)
{
    try
    {
        if (!output.Exists)
            output.Create();

        using var rsa = RSA.Create(2048);

        // Private key (PKCS#8 PEM)
        var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
        var privateKeyB64 = Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks);
        var privateKeyPem = $"-----BEGIN PRIVATE KEY-----\n{privateKeyB64}\n-----END PRIVATE KEY-----";
        var privateKeyPath = Path.Combine(output.FullName, "private-key.pem");
        File.WriteAllText(privateKeyPath, privateKeyPem);

        // Public key (SubjectPublicKeyInfo PEM)
        var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
        var publicKeyB64 = Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks);
        var publicKeyPem = $"-----BEGIN PUBLIC KEY-----\n{publicKeyB64}\n-----END PUBLIC KEY-----";
        var publicKeyPath = Path.Combine(output.FullName, "public-key.pem");
        File.WriteAllText(publicKeyPath, publicKeyPem);

        Console.WriteLine($"RSA 2048-bit key pair generated:");
        Console.WriteLine($"  Private key: {privateKeyPath}");
        Console.WriteLine($"  Public key : {publicKeyPath}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error generating keys: {ex.Message}");
        Environment.Exit(1);
    }
}

static void ValidateHandler(FileInfo config)
{
    try
    {
        if (!config.Exists)
        {
            Console.Error.WriteLine($"Config file not found: {config.FullName}");
            Environment.Exit(1);
        }

        var json = File.ReadAllText(config.FullName);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var fieldChecks = new (string Label, string[] Keys)[]
        {
            ("baseUrl",    ["baseUrl", "BaseUrl"]),
            ("clientId",   ["clientId", "ClientId"]),
            ("privateKeyPath", ["privateKeyPath", "PrivateKeyPem"]),
            ("channelId",  ["channelId", "ChannelId"]),
            ("partnerId",  ["partnerId", "PartnerId"]),
        };
        var allValid = true;

        Console.WriteLine($"Validating config: {config.FullName}");
        Console.WriteLine();

        var checkedKeyPaths = new Dictionary<string, string>();

        foreach (var (label, keys) in fieldChecks)
        {
            string? value = null;
            foreach (var k in keys)
            {
                if (root.TryGetProperty(k, out var v) && v.ValueKind == JsonValueKind.String)
                {
                    value = v.GetString();
                    checkedKeyPaths[label] = k;
                    break;
                }
            }

            var hasField = !string.IsNullOrWhiteSpace(value);
            Console.WriteLine($"  [{(hasField ? "PASS" : "FAIL")}] {label}");
            if (!hasField) allValid = false;
        }

        // Validate private key file
        Console.WriteLine();
        var privateKeyPath = root.TryGetProperty("privateKeyPath", out var pkv)
            ? pkv.GetString()
            : root.TryGetProperty("PrivateKeyPem", out var pkv2)
                ? pkv2.GetString()
                : null;

        if (!string.IsNullOrWhiteSpace(privateKeyPath))
        {
            // Resolve relative path relative to config file directory
            var resolvedPath = Path.IsPathRooted(privateKeyPath)
                ? privateKeyPath
                : Path.GetFullPath(Path.Combine(config.DirectoryName ?? ".", privateKeyPath));

            var keyExists = File.Exists(resolvedPath);
            var validPem = false;

            if (keyExists)
            {
                try
                {
                    var pemContent = File.ReadAllText(resolvedPath);
                    using var rsa = RSA.Create();
                    rsa.ImportFromPem(pemContent);
                    validPem = true;
                }
                catch
                {
                    validPem = false;
                }
            }

            Console.WriteLine($"  [{(keyExists && validPem ? "PASS" : "FAIL")}] Private key file: {resolvedPath}");
            Console.WriteLine($"      File exists: {keyExists}");
            Console.WriteLine($"      Valid PEM  : {validPem}");

            if (!keyExists || !validPem) allValid = false;
        }

        Console.WriteLine();
        Console.WriteLine($"Overall: {(allValid ? "VALID" : "INVALID")}");

        if (!allValid)
            Environment.Exit(1);
    }
    catch (JsonException ex)
    {
        Console.Error.WriteLine($"Invalid JSON: {ex.Message}");
        Environment.Exit(1);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Validation error: {ex.Message}");
        Environment.Exit(1);
    }
}

static async Task SandboxTokenHandler(FileInfo config)
{
    await SandboxApiPost(config, "v1.0/access-token/b2b",
        new { grantType = "client_credentials" });
}

static async Task SandboxBalanceHandler(string account, FileInfo config)
{
    await SandboxApiPost(config, "v1.0/balance-inquiry",
        new
        {
            partnerReferenceNo = Guid.NewGuid().ToString(),
            accountNo = account,
        });
}

static async Task SandboxTransferHandler(
    string to, string amount, string? bank, string name, string? remark, string? source, FileInfo config)
{
    var body = new Dictionary<string, object?>
    {
        ["partnerReferenceNo"] = Guid.NewGuid().ToString(),
        ["amount"] = new { value = amount, currency = "IDR" },
        ["beneficiaryAccountNo"] = to,
        ["beneficiaryName"] = name,
    };

    if (!string.IsNullOrWhiteSpace(bank))
        body["beneficiaryBankCode"] = bank;
    if (!string.IsNullOrWhiteSpace(remark))
        body["remark"] = remark;
    if (!string.IsNullOrWhiteSpace(source))
        body["sourceAccountNo"] = source;

    await SandboxApiPost(config, "v1.0/transfer-va/credit", body);
}

static async Task SandboxVaCreateHandler(
    string partnerServiceId, string customerNo, string name, string amount, string expired, FileInfo config)
{
    // Use WIB time for transaction date
    var txnDate = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
        .ToString("yyyy-MM-ddTHH:mm:sszzz");

    var body = new
    {
        partnerServiceId,
        customerNo,
        virtualAccountNo = $"{partnerServiceId}{customerNo}",
        virtualAccountName = name,
        virtualAccountEmail = "",
        virtualAccountPhone = "",
        trxId = Guid.NewGuid().ToString("N"),
        totalAmount = new { value = amount, currency = "IDR" },
        additionalInfo = new { },
        expiredDate = expired,
        txnDate,
    };

    await SandboxApiPost(config, "v1.0/transfer-va/create-va", body);
}

static async Task SandboxQrisHandler(string amount, string merchantId, string storeId, FileInfo config)
{
    var body = new
    {
        partnerReferenceNo = Guid.NewGuid().ToString(),
        amount = new { value = amount, currency = "IDR" },
        merchantId,
        storeId,
        validityPeriod = "2026-12-31T23:59:59+07:00",
        additionalInfo = new { },
    };

    await SandboxApiPost(config, "v1.0/qr/qr-generate", body);
}

static async Task SandboxApiPost(FileInfo config, string path, object body)
{
    try
    {
        if (!config.Exists)
        {
            Console.Error.WriteLine($"Config file not found: {config.FullName}");
            Environment.Exit(1);
        }

        var cfg = LoadConfig(config);
        var jsonBody = JsonSerializer.Serialize(body, CliHelper.JsonOpts);
        var rsa = LoadPrivateKey(cfg.PrivateKeyPem, config);

        var timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
            .ToString("yyyy-MM-ddTHH:mm:sszzz");

        var bodyHash = ComputeSha256Hex(jsonBody);
        var stringToSign = $"POST:{path}:{bodyHash}:{timestamp}";
        var signature = ComputeRsaSha256Signature(rsa, stringToSign);

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(cfg.BaseUrl.TrimEnd('/') + "/");
        httpClient.DefaultRequestHeaders.Add("X-TIMESTAMP", timestamp);
        httpClient.DefaultRequestHeaders.Add("X-SIGNATURE", signature);
        httpClient.DefaultRequestHeaders.Add("X-CLIENT-KEY", cfg.ClientId);
        httpClient.DefaultRequestHeaders.Add("X-PARTNER-ID", cfg.PartnerId);
        httpClient.DefaultRequestHeaders.Add("CHANNEL-ID", cfg.ChannelId);
        httpClient.DefaultRequestHeaders.Add("X-EXTERNAL-ID", Guid.NewGuid().ToString("N"));

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(path, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.Error.WriteLine();
            PrintJson(responseBody);
            Environment.Exit(1);
        }

        PrintJson(responseBody);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.Exit(1);
    }
}

static void SignHandler(
    string method, string path, FileInfo? body, FileInfo key, string? timestamp)
{
    try
    {
        if (!key.Exists)
        {
            Console.Error.WriteLine($"Private key file not found: {key.FullName}");
            Environment.Exit(1);
        }

        timestamp ??= DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
            .ToString("yyyy-MM-ddTHH:mm:sszzz");

        var bodyContent = body is not null && body.Exists
            ? File.ReadAllText(body.FullName)
            : string.Empty;

        var bodyHash = ComputeSha256Hex(bodyContent);
        var stringToSign = $"{method.ToUpperInvariant()}:{path}:{bodyHash}:{timestamp}";

        using var rsa = RSA.Create();
        var pemContent = File.ReadAllText(key.FullName);
        rsa.ImportFromPem(pemContent);

        var signature = ComputeRsaSha256Signature(rsa, stringToSign);

        Console.WriteLine("=== Signature Debug ===");
        Console.WriteLine();
        Console.WriteLine($"HTTP Method   : {method}");
        Console.WriteLine($"Path          : {path}");
        Console.WriteLine($"Timestamp     : {timestamp}");
        Console.WriteLine();
        Console.WriteLine($"Body          : {(body is not null ? body.FullName : "(empty)")}");
        Console.WriteLine($"Body Content  : {bodyContent}");
        Console.WriteLine();
        Console.WriteLine("=== Computed Values ===");
        Console.WriteLine($"Body Hash     : {bodyHash}");
        Console.WriteLine($"String to Sign: {stringToSign}");
        Console.WriteLine($"Signature     : {signature}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error generating signature: {ex.Message}");
        Environment.Exit(1);
    }
}


// ═══════════════════════════════════════════════════════════════════════════
//  Helpers — Local Functions
// ═══════════════════════════════════════════════════════════════════════════

static SnapSharpConfig LoadConfig(FileInfo configFile)
{
    var json = File.ReadAllText(configFile.FullName);
    var doc = JsonDocument.Parse(json);
    var root = doc.RootElement;

    string GetString(string prop) =>
        root.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String
            ? val.GetString() ?? ""
            : "";

    var baseUrl = GetString("baseUrl") ?? GetString("BaseUrl") ?? "";
    var clientId = GetString("clientId") ?? GetString("ClientId") ?? "";
    var privateKeyPath = GetString("privateKeyPath") ?? GetString("PrivateKeyPem") ?? "";
    var channelId = GetString("channelId") ?? GetString("ChannelId") ?? "";
    var partnerId = GetString("partnerId") ?? GetString("PartnerId") ?? "";

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("BaseUrl is required in config.");

    // Resolve private key path relative to config file
    var resolvedKeyPath = Path.IsPathRooted(privateKeyPath)
        ? privateKeyPath
        : Path.GetFullPath(Path.Combine(configFile.DirectoryName ?? ".", privateKeyPath));

    if (!File.Exists(resolvedKeyPath))
        throw new InvalidOperationException($"Private key file not found: {resolvedKeyPath}");

    var pemContent = File.ReadAllText(resolvedKeyPath);

    return new SnapSharpConfig(baseUrl, clientId, pemContent, channelId, partnerId);
}

static RSA LoadPrivateKey(string privateKeyPem, FileInfo configFile)
{
    var rsa = RSA.Create();
    rsa.ImportFromPem(privateKeyPem);
    return rsa;
}

static string ComputeSha256Hex(string input)
{
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
    return Convert.ToHexString(hash).ToLowerInvariant();
}

static string ComputeRsaSha256Signature(RSA rsa, string data)
{
    var signature = rsa.SignData(
        Encoding.UTF8.GetBytes(data),
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);
    return Convert.ToBase64String(signature);
}

static void PrintJson(string json)
{
    try
    {
        using var doc = JsonDocument.Parse(json);
        var formatted = JsonSerializer.Serialize(doc.RootElement, CliHelper.JsonOpts);
        Console.WriteLine(formatted);
    }
    catch
    {
        Console.WriteLine(json);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
//  Types
// ═══════════════════════════════════════════════════════════════════════════

sealed record SnapSharpConfig(
    string BaseUrl,
    string ClientId,
    string PrivateKeyPem,
    string ChannelId,
    string PartnerId);

internal static class CliHelper
{
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}

