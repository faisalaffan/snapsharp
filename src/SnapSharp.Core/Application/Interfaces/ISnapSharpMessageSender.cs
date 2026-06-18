namespace SnapSharp.Application.Interfaces;

public interface ISnapSharpMessageSender
{
    Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object? body, CancellationToken ct)
        where TResponse : class;
}
