using Discord;
using TheGuild.DataLayer.Guilds;
using TheGuild.DataLayer.Models.Guilds;
using GuildPermissions = TheGuild.DataLayer.Models.Guilds.GuildPermissions;

namespace TheGuild.Api.Authorization;

public interface IGuildPermissionsProvider
{
    Task<GuildPermissions> GetAsync(ulong discordServerId, ulong discordUserId);
}

public class GuildPermissionsProvider : IGuildPermissionsProvider
{
    private readonly IGuildRepository _guildRepository;
    private readonly IDiscordClient _discordClient;

    public GuildPermissionsProvider(IGuildRepository guildRepository, IDiscordClient discordClient)
    {
        _guildRepository = guildRepository;
        _discordClient = discordClient;
    }

    public async Task<GuildPermissions> GetAsync(ulong discordServerId, ulong discordUserId)
    {
        var guild = await _guildRepository.FindByDiscordServerId(discordServerId);

        if (guild is null)
        {
            throw new KeyNotFoundException($"Not found registered guild {discordServerId}");
        }

        var discordServer = await _discordClient.GetGuildAsync(discordServerId, CacheMode.AllowDownload, RequestOptions.Default);
        if (discordServer is null)
        {
            throw new KeyNotFoundException($"Not found Discord server {discordServerId}");
        }

        var user = await discordServer.GetUserAsync(discordUserId, CacheMode.AllowDownload, RequestOptions.Default);
        if (user is null)
        {
            throw new KeyNotFoundException($"Not found user {discordUserId} on Discord server {discordServerId}");
        }

        var userSpecificPermissions = guild.GuildRoles
            .Where(x => x.DiscordUsers?.Contains(discordUserId) ?? false)
            .Select(x => x.Permissions)
            .Merge();

        var discordRolesPermissions = user.RoleIds
            .SelectMany(discordRoleId => guild.GuildRoles
                .Where(guildRole => guildRole.DiscordRoles?.Contains(discordRoleId) ?? false)
                .Select(x => x.Permissions))
            .Merge();

        return userSpecificPermissions.Merge(discordRolesPermissions);
    }
}