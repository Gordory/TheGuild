using Microsoft.Extensions.Caching.Memory;
using TheGuild.DataLayer.Guilds;
using TheGuild.DataLayer.Models.Access;
using TheGuild.DataLayer.Models.Guilds;

namespace TheGuild.Api.Authorization;

public interface IGuildActorProvider
{
    Task<GuildActor> GetAsync(ulong discordServerId, ulong discordUserId);
}

public sealed class GuildActorProvider : IGuildActorProvider
{
    // Short enough that an officer editing roles sees the effect without restarting anything, long
    // enough that a burst of requests reads the guild once.
    private static readonly TimeSpan GuildCacheLifetime = TimeSpan.FromSeconds(60);

    private readonly IGuildRepository _guildRepository;
    private readonly IGuildMemberRolesReader _memberRolesReader;
    private readonly IMemoryCache _cache;

    public GuildActorProvider(
        IGuildRepository guildRepository,
        IGuildMemberRolesReader memberRolesReader,
        IMemoryCache cache)
    {
        _guildRepository = guildRepository;
        _memberRolesReader = memberRolesReader;
        _cache = cache;
    }

    public async Task<GuildActor> GetAsync(ulong discordServerId, ulong discordUserId)
    {
        var guild = await GetGuildAsync(discordServerId);

        // An unregistered server grants nothing. Failing closed here rather than throwing keeps an
        // unconfigured guild a 403 instead of a 500.
        if (guild is null)
        {
            return new GuildActor
            {
                DiscordServerId = discordServerId,
                DiscordUserId = discordUserId,
            };
        }

        var memberRoleIds = await _memberRolesReader.GetRoleIdsAsync(discordServerId, discordUserId);

        var grantingRoles = guild.GuildRoles
            .Where(role => Matches(role, discordUserId, memberRoleIds))
            .Where(role => role.Grants.Length > 0)
            .ToArray();

        var granted = grantingRoles
            .SelectMany(role => role.Grants)
            .Select(grant => grant.PermissionId);

        return new GuildActor
        {
            DiscordServerId = discordServerId,
            DiscordUserId = discordUserId,
            Permissions = PermissionCatalog.Expand(granted),
            GrantedBy = grantingRoles.Select(role => role.Id).ToArray(),
        };
    }

    private static bool Matches(GuildRole role, ulong discordUserId, ulong[] memberRoleIds)
    {
        return role.DiscordUserIds.Contains(discordUserId)
               || role.DiscordRoleIds.Any(memberRoleIds.Contains);
    }

    private async Task<Guild?> GetGuildAsync(ulong discordServerId)
    {
        var cacheKey = (nameof(GuildActorProvider), discordServerId);

        if (_cache.TryGetValue(cacheKey, out Guild? cached))
        {
            return cached;
        }

        var guild = await _guildRepository.FindByDiscordServerId(discordServerId);
        _cache.Set(cacheKey, guild, GuildCacheLifetime);

        return guild;
    }
}
