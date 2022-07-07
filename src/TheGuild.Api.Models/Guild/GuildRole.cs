namespace TheGuild.Api.Models.Guild;

public class GuildRole
{
    public ulong[] DiscordRoles { get; init; }

    public GuildPermissions Permissions { get; init; }
}