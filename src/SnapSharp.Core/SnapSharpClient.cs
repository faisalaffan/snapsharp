using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SnapSharp.Authentication;
using SnapSharp.Exceptions;
using SnapSharp.Http;
using SnapSharp.Services;

namespace SnapSharp;

public sealed class SnapSharpClient : ISnapSharpClient
{
    private readonly SnapSharpOptions _options;
    private readonly HttpClient _httpClient;
    private AccessTokenResponse? _currentToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public IAuthService Auth { get; }
    public IAccountService Account { get; }
    public ITransferService Transfer { get; }
    public IVirtualAccountService VirtualAccount { get; }
    public IQrisService Qris { get; }
    public IDirectDebitService DirectDebit { get; }

    public SnapSharpClient(SnapSharpOptions options) : this(options, null) { }

    internal SnapSharpClient(SnapSharpOptions options, HttpMessageHandler? handler)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            throw new ArgumentException("BaseUrl is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ClientId))
            throw new ArgumentException("ClientId is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.PrivateKeyPem))
            throw new ArgumentException("PrivateKeyPem is required.", nameof(options));

        var innerHandler = handler ?? new HttpClientHandler();
        var signingHandler = new SnapSharpHttpHandler(options, innerHandler);
        _httpClient = new HttpClient(signingHandler)
        {
            BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds),
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        Auth = new AuthService(this);
        Account = new AccountService(this);
        Transfer = new TransferService(this);
        VirtualAccount = new VirtualAccountService(this);
        Qris = new QrisService(this);
        DirectDebit = new DirectDebitService(this);
    }

    internal async Task<string> GetValidAccessTokenAsync(CancellationToken ct)
    {
        if (_currentToken is { IsExpired: false })
            return _currentToken.AccessToken;

        await _tokenLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_currentToken is { IsExpired: false })
                return _currentToken.AccessToken;

            _currentToken = await Auth.GetAccessTokenAsync(ct).ConfigureAwait(false);
            return _currentToken.AccessToken;
        }
        finally { _tokenLock.Release(); }
    }

    internal async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class
    {
        var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, SnapSharpJsonContext.Default.Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var error = JsonSerializer.Deserialize<SnapSharpErrorResponse>(
                    responseBody, SnapSharpJsonContext.Default.Options);
                if (error is not null)
                    throw new SnapSharpApiException(
                        (int)response.StatusCode, error.ResponseCode, error.ResponseMessage);
            }
            catch (JsonException) { }

            throw new SnapSharpApiException(
                (int)response.StatusCode,
                ((int)response.StatusCode).ToString(),
                responseBody);
        }

        var result = JsonSerializer.Deserialize<TResponse>(
            responseBody, SnapSharpJsonContext.Default.Options);

        return result ?? throw new SnapSharpException(
            $"Deserialization returned null for {typeof(TResponse).Name}");
    }

    internal TResponse Send<TResponse>(HttpMethod method, string path, object? body)
        where TResponse : class
    {
        return SyncHelper.Run(() => SendAsync<TResponse>(method, path, body, CancellationToken.None));
    }

    internal SnapSharpOptions Options => _options;

    public void Dispose()
    {
        _httpClient.Dispose();
        _tokenLock.Dispose();
    }
}
