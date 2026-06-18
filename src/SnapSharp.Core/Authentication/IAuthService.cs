using SnapSharp.Models;

namespace SnapSharp.Authentication;

public interface IAuthService
{
    Task<AccessTokenResponse> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    Task<AccessTokenResponse> GetAccessTokenB2b2cAsync(
        string customerNo,
        string accountNo,
        Dictionary<string, string>? additionalInfo = null,
        CancellationToken cancellationToken = default);
}
