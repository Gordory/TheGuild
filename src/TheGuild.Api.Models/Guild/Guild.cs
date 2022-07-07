namespace TheGuild.Api.Models.Guild;

public record Guild
{
    public Guid Id { get; init; }

    public string? UniqueName { get; init; }
    
    public ulong DiscordServerId { get; init; }

    public string Name { get; init; }

    public GuildRole[] GuildRoles { get; init; }
}