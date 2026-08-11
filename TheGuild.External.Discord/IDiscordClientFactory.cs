using Discord;
using Discord.Rest;
using Microsoft.Extensions.Options;
using TheGuild.External.Discord.Configuration;

namespace TheGuild.External.Discord;

public interface IDiscordClientFactory
{
    Task<IDiscordClient> CreateAsync();
}

public class DiscordClientFactory : IDiscordClientFactory
{
    private readonly IOptions<DiscordBotConfiguration> _configuration;

    public DiscordClientFactory(IOptions<DiscordBotConfiguration> configuration)
    {
        _configuration = configuration;
    }

    public async Task<IDiscordClient> CreateAsync()
    {
        var client = new DiscordRestClient(
            new DiscordRestConfig
            {
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

        var botToken = _configuration.Value.Token;
        await client.LoginAsync(TokenType.Bot, botToken).ConfigureAwait(false);

        return client;
    }
}