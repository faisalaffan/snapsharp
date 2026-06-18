using System.Security.Cryptography;
using System.Text;
using SnapSharp.Exceptions;

namespace SnapSharp.Http;

internal sealed class SnapSharpHttpHandler : DelegatingHandler
{
    private readonly SnapSharpOptions _options;
    private readonly RSA _rsa;

    public SnapSharpHttpHandler(SnapSharpOptions options, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _options = options;
        _rsa = RSA.Create();
        _rsa.ImportFromPem(options.PrivateKeyPem);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7))
            .ToString("yyyy-MM-ddTHH:mm:sszzz");

        var bodyContent = request.Content is not null
            ? await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)
            : string.Empty;
        var bodyHash = ComputeSha256Hex(bodyContent).ToLowerInvariant();

        var path = request.RequestUri!.PathAndQuery;
        var stringToSign = $"{request.Method}:{path}:{bodyHash}:{timestamp}";

        var signature = ComputeRsaSha256Signature(stringToSign);

        request.Headers.Add("X-TIMESTAMP", timestamp);
        request.Headers.Add("X-SIGNATURE", signature);
        request.Headers.Add("X-CLIENT-KEY", _options.ClientId);
        request.Headers.Add("X-PARTNER-ID", _options.PartnerId);
        request.Headers.Add("CHANNEL-ID", _options.ChannelId);
        request.Headers.Add("X-EXTERNAL-ID", Guid.NewGuid().ToString("N"));

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static string ComputeSha256Hex(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }

    private string ComputeRsaSha256Signature(string data)
    {
        try
        {
            var signature = _rsa.SignData(
                Encoding.UTF8.GetBytes(data),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signature);
        }
        catch (Exception ex)
        {
            throw new SnapSharpSignatureException("Failed to compute RSA-SHA256 signature.", ex);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _rsa.Dispose();
        base.Dispose(disposing);
    }
}