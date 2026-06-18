using SnapSharp;
using SnapSharp.Application.Contracts.Account;
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Application.Contracts.DirectDebit;
using SnapSharp.Application.Contracts.Qris;
using SnapSharp.Application.Contracts.Transfer;
using SnapSharp.Application.Contracts.VirtualAccount;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Domain.Options;

var builder = WebApplication.CreateBuilder(args);

// ── SnapSharp Client ──────────────────────────────────────────────────────────
var snapSection = builder.Configuration.GetSection("SnapSharp");
var snapOptions = new SnapSharpOptions
{
    BaseUrl = snapSection["BaseUrl"] ?? "https://sandbox.bank.co.id",
    ClientId = snapSection["ClientId"] ?? "",
    PrivateKeyPem = snapSection["PrivateKeyPem"] ?? "",
    ChannelId = snapSection["ChannelId"] ?? "95221",
    PartnerId = snapSection["PartnerId"] ?? "",
    TimeoutSeconds = int.Parse(snapSection["TimeoutSeconds"] ?? "30"),
    MaxRetries = int.Parse(snapSection["MaxRetries"] ?? "3"),
};

builder.Services.AddSnapSharp(snapOptions);

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SnapSharp Reference API",
        Version = "v1",
        Description = "Reference implementation for BI SNAP integration using SnapSharp SDK"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SnapSharp v1");
        options.RoutePrefix = "swagger";
    });
}

// ── Health Check ─────────────────────────────────────────────────────────────
app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
        .ToString("yyyy-MM-ddTHH:mm:sszzz")
}))
.WithTags("Health")
.WithOpenApi();

// ── Auth Endpoints ───────────────────────────────────────────────────────────
var authGroup = app.MapGroup("/auth")
    .WithTags("Authentication");

authGroup.MapPost("/token", async (ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var token = await client.Auth.GetAccessTokenAsync(ct);
        return Results.Ok(token);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Get B2B access token";
    return ops;
});

authGroup.MapPost("/token/b2b2c", async (B2b2cAuthRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var token = await client.Auth.GetAccessTokenB2b2cAsync(
            req.CustomerNo, req.AccountNo, req.AdditionalInfo, ct);
        return Results.Ok(token);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Get B2B2C access token";
    return ops;
});

// ── Account Endpoints ────────────────────────────────────────────────────────
var accountGroup = app.MapGroup("/account")
    .WithTags("Account");

accountGroup.MapPost("/inquiry", async (AccountRegistrationRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Account.InquiryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Account registration inquiry";
    return ops;
});

accountGroup.MapPost("/balance", async (AccountBalanceRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Account.GetBalanceAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Balance inquiry";
    return ops;
});

// ── Transfer Endpoints ───────────────────────────────────────────────────────
var transferGroup = app.MapGroup("/transfer")
    .WithTags("Transfer");

transferGroup.MapPost("/credit", async (CreditTransferRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Transfer.CreditTransferAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Credit transfer (internal & interbank)";
    return ops;
});

transferGroup.MapPost("/history", async (TransactionHistoryRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Transfer.GetHistoryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Transaction history";
    return ops;
});

// ── Virtual Account Endpoints ────────────────────────────────────────────────
var vaGroup = app.MapGroup("/va")
    .WithTags("Virtual Account");

vaGroup.MapPost("/create", async (CreateVaRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.CreateAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Create virtual account";
    return ops;
});

vaGroup.MapPost("/inquiry", async (VAInquiryRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.InquiryAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Virtual account inquiry";
    return ops;
});

vaGroup.MapPost("/payment", async (VAPaymentNotifyRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.VirtualAccount.PaymentNotifyAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Virtual account payment notification";
    return ops;
});

// ── QRIS Endpoints ───────────────────────────────────────────────────────────
var qrisGroup = app.MapGroup("/qris")
    .WithTags("QRIS");

qrisGroup.MapPost("/generate", async (QrisGenerateRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Qris.GenerateAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Generate QRIS";
    return ops;
});

qrisGroup.MapPost("/payment", async (QrisPaymentNotifyRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.Qris.PaymentNotifyAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "QRIS payment notification";
    return ops;
});

// ── Direct Debit Endpoints ───────────────────────────────────────────────────
var debitGroup = app.MapGroup("/debit")
    .WithTags("Direct Debit");

debitGroup.MapPost("/register", async (DirectDebitRegisterRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.DirectDebit.RegisterAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Direct debit registration";
    return ops;
});

debitGroup.MapPost("/payment", async (DirectDebitPaymentRequest req, ISnapSharpClient client, CancellationToken ct) =>
{
    try
    {
        var result = await client.DirectDebit.PaymentAsync(req, ct);
        return Results.Ok(result);
    }
    catch (SnapSharpException ex)
    {
        return Results.BadRequest(new { error = ex.Message, code = ex.ResponseCode });
    }
})
.WithOpenApi(ops =>
{
    ops.Summary = "Direct debit payment";
    return ops;
});

app.Run();

// ── DTO ──────────────────────────────────────────────────────────────────────
internal sealed record B2b2cAuthRequest(
    string CustomerNo,
    string AccountNo,
    Dictionary<string, string>? AdditionalInfo = null);
