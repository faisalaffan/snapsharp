using SnapSharp.Application.Contracts.Auth;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;

namespace SnapSharp.Application.Services;

internal sealed class AuthService : IAuthService
{
    private readonly ISnapSharpMessageSender _sender;
    private const string TokenPathB2B = "v1.0/access-token/b2b";
    private const string TokenPathB2B2C = "v1.0/access-token/b2b2c";

    public AuthService(ISnapSharpMessageSender sender)
    {
        _sender = sender;
    }

    public async Task<AccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var body = new { grantType = "client_credentials" };

        var response = await _sender.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B, body, cancellationToken);

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

        var response = await _sender.SendAsync<AccessTokenResponse>(
            HttpMethod.Post, TokenPathB2B2C, body, cancellationToken);

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
