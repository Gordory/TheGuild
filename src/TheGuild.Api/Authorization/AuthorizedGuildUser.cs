using TheGuild.Api.Models.Guild;
using TheGuild.Api.Models.Users.Discord;

namespace TheGuild.Api.Authorization;

public class AuthorizedGuildUser
{
    public Guild Guild { get; init; }

    public DiscordUser User { get; init; }

    public GuildPermissions Permissions { get; init; }
}