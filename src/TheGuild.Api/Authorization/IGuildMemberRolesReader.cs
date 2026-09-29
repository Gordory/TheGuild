using Discord;
using Microsoft.Extensions.Caching.Memory;

namespace TheGuild.Api.Authorization;

/// <summary>
/// Narrow port onto the one thing authorization needs from Discord. Keeping it this small is what
/// lets the caching, the absent-member case and the test doubles live in one place instead of being
/// spread across everything that touches <see cref="IDiscordClient"/>.
/// </summary>
public interface IGuildMemberRolesReader
{
    Task<ulong[]> GetRoleIdsAsync(ulong discordServerId, ulong discordUserId);
}

public sealed class DiscordGuildMemberRolesReader : IGuildMemberRolesReader
{
    // Role membership is asked for on every authorization, and Discord rate-limits per bot rather
    // than per guild, so uncached reads would be the first thing to break under load.
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromMinutes(5);

    private readonly IDiscordClient _discordClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DiscordGuildMemberRolesReader> _logger;

    public DiscordGuildMemberRolesReader(
        IDiscordClient discordClient,
        IMemoryCache cache,
        ILogger<DiscordGuildMemberRolesReader> logger)
    {
        _discordClient = discordClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ulong[]> GetRoleIdsAsync(ulong discordServerId, ulong discordUserId)
    {
        var cacheKey = (nameof(DiscordGuildMemberRolesReader), discordServerId, discordUserId);

        if (_cache.TryGetValue(cacheKey, out ulong[]? cached))
        {
            return cached!;
        }

        var roleIds = await ReadRoleIdsAsync(discordServerId, discordUserId);
        _cache.Set(cacheKey, roleIds, CacheLifetime);

        return roleIds;
    }

    private async Task<ulong[]> ReadRoleIdsAsync(ulong discordServerId, ulong discordUserId)
    {
        var discordServer = await _discordClient.GetGuildAsync(
            discordServerId,
            CacheMode.AllowDownload,
            RequestOptions.Default);

        if (discordServer is null)
        {
            // The bot is not on that server, or the guild is registered against the wrong id. Both
            // end in no permissions, but silence would make either impossible to diagnose.
            _logger.LogWarning(
                "Discord server {DiscordServerId} is not reachable by the bot; treating {DiscordUserId} as having no roles",
                discordServerId,
                discordUserId);

            return Array.Empty<ulong>();
        }

        var member = await discordServer.GetUserAsync(
            discordUserId,
            CacheMode.AllowDownload,
            RequestOptions.Default);

        // Someone who left the server simply holds no roles; that is ordinary, not a failure.
        return member?.RoleIds.ToArray() ?? Array.Empty<ulong>();
    }
}
