using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SnapSharp.Application.Interfaces;
using SnapSharp.Domain.Exceptions;
using SnapSharp.Infrastructure.Serialization;
using SnapSharp.Infrastructure.Token;

namespace SnapSharp.Infrastructure.Messaging;

internal sealed class SnapSharpMessageSender : ISnapSharpMessageSender
{
    private readonly HttpClient _httpClient;
    private readonly TokenManager _tokenManager;

    public SnapSharpMessageSender(HttpClient httpClient, TokenManager tokenManager)
    {
        _httpClient = httpClient;
        _tokenManager = tokenManager;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class
    {
        var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, SnapSharpJsonContext.Instance.Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var token = await _tokenManager.GetValidAccessTokenAsync(ct);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var error = JsonSerializer.Deserialize<SnapSharpErrorResponse>(
                    responseBody, SnapSharpJsonContext.Instance.Options);
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
            responseBody, SnapSharpJsonContext.Instance.Options);

        return result ?? throw new SnapSharpException(
            $"Deserialization returned null for {typeof(TResponse).Name}");
    }
}
