using SnapSharp.Exceptions;
using SnapSharp.Models;

namespace SnapSharp.Authentication;

internal sealed class AuthService : IAuthService
{
    private readonly SnapSharpClient _client;
    private const string TokenPathB2B = "v1.0/access-token/b2b";
    private const string TokenPathB2B2C = "v1.0/access-token/b2b2c";

    public AuthService(SnapSharpClient client)
    {
        _client = client;
    }

    public async Task<AccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var body = new { grantType = "client_credentials" };

        var response = await _client.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B, body, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new SnapSharpAuthenticationException(
                response.ResponseCode,
                $"Access token is empty. {response.ResponseMessage}");
        }

        return response with
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 30)
        };
    }

    public async Task<AccessTokenResponse> GetAccessTokenB2b2cAsync(
        string customerNo,
        string accountNo,
        Dictionary<string, string>? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        var body = new B2b2cTokenRequest
        {
            CustomerNo = customerNo,
            AccountNo = accountNo,
            AdditionalInfo = additionalInfo
        };

        var response = await _client.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B2C, body, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(response.AccessToken))
        {
            throw new SnapSharpAuthenticationException(
                response.ResponseCode,
                $"B2B2C access token is empty. {response.ResponseMessage}");
        }

        return response with
        {
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn - 30)
        };
    }
}
