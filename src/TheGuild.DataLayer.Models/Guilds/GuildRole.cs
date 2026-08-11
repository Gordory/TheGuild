namespace TheGuild.DataLayer.Models.Guilds;

public class GuildRole
{
    public ulong[]? DiscordUsers { get; init; }
    
    public ulong[]? DiscordRoles { get; init; }

    public GuildPermissions Permissions { get; init; }
}