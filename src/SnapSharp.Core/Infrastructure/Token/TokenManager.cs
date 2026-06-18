using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Infrastructure.Serialization;

namespace SnapSharp.Infrastructure.Token;

internal sealed class TokenManager : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private AccessTokenResponse? _currentToken;

    public TokenManager(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetValidAccessTokenAsync(CancellationToken ct)
    {
        if (_currentToken is { IsExpired: false })
            return _currentToken.AccessToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_currentToken is { IsExpired: false })
                return _currentToken.AccessToken;

            var body = JsonSerializer.Serialize(
                new { grantType = "client_credentials" },
                SnapSharpJsonContext.Instance.Options);

            var request = new HttpRequestMessage(HttpMethod.Post, "v1.0/access-token/b2b")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new SnapSharpAuthenticationException(
                    ((int)response.StatusCode).ToString(),
                    $"Token request failed: {responseBody}");
            }

            var token = JsonSerializer.Deserialize<AccessTokenResponse>(
                responseBody, SnapSharpJsonContext.Instance.Options);

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new SnapSharpAuthenticationException(
                    token?.ResponseCode ?? "UNKNOWN",
                    $"Access token is empty. {token?.ResponseMessage}");
            }

            _currentToken = token with
            {
                ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 30)
            };

            return _currentToken.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public void Dispose() => _tokenLock.Dispose();
}
