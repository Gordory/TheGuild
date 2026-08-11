namespace TheGuild.DataLayer.Models.Authentication;

// Do not rename class
public class DiscordBotApiKeyBinding : ApiKeyBinding
{
    public ulong DiscordBotClientId { get; init; }
}