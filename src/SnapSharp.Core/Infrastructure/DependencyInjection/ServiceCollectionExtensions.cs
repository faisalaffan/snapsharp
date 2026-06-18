using System.Net.Http.Headers;
using SnapSharp;
using SnapSharp.Application.Interfaces;
using SnapSharp.Application.Services;
using SnapSharp.Domain.Options;
using SnapSharp.Infrastructure.Http;
using SnapSharp.Infrastructure.Messaging;
using SnapSharp.Infrastructure.Token;

namespace Microsoft.Extensions.DependencyInjection;

public static class SnapSharpServiceCollectionExtensions
{
    public static IServiceCollection AddSnapSharp(
        this IServiceCollection services, SnapSharpOptions options)
    {
        services.AddSingleton(options);

        // TokenManager — own typed HttpClient for direct token endpoint access (avoids circular dep)
        services.AddHttpClient<TokenManager>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler<SnapSharpHttpHandler>();

        // SnapSharpMessageSender — typed HttpClient for business API calls
        services.AddHttpClient<ISnapSharpMessageSender, SnapSharpMessageSender>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<SnapSharpHttpHandler>();

        // Services
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAccountService, AccountService>();
        services.AddSingleton<ITransferService, TransferService>();
        services.AddSingleton<IVirtualAccountService, VirtualAccountService>();
        services.AddSingleton<IQrisService, QrisService>();
        services.AddSingleton<IDirectDebitService, DirectDebitService>();

        // Client facade
        services.AddSingleton<ISnapSharpClient, SnapSharpClient>();

        return services;
    }
}
