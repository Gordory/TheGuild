using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheGuild.External.Discord.Configuration;

namespace TheGuild.External.Discord;

/// <summary>
/// Counterpart of <see cref="ServiceRegistryExtensions"/> for hosts built on Microsoft DI,
/// such as the API. The bot keeps using the LightInject registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DiscordBotConfiguration>(configuration.GetSection(DiscordBotConfiguration.ConfigPath));

        services.AddSingleton<IDiscordClientFactory, DiscordClientFactory>();

        // Logging in is asynchronous, and the client is resolved lazily, so the first resolution
        // pays for it. Discord.Net keeps the session alive for the lifetime of the singleton.
        services.AddSingleton(serviceProvider => serviceProvider
            .GetRequiredService<IDiscordClientFactory>()
            .CreateAsync()
            .GetAwaiter()
            .GetResult());

        return services;
    }
}
