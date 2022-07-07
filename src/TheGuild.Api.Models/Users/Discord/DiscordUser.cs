namespace TheGuild.Api.Models.Users.Discord;

public record DiscordUser : User
{
    public ulong DiscordUserId { get; init; }

    public string? LastKnownTag { get; init; }
}