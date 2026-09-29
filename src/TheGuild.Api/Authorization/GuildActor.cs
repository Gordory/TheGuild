namespace TheGuild.Api.Authorization;

/// <summary>
/// Who is acting, and what they may do, resolved once per request. Holds no Discord or database
/// handles, so authorization decisions are a pure function of it plus the resource at hand.
/// </summary>
public sealed class GuildActor
{
    public ulong DiscordServerId { get; init; }

    public ulong DiscordUserId { get; init; }

    public IReadOnlySet<string> Permissions { get; init; } = new HashSet<string>();

    /// <summary>
    /// Roles the permissions came from, so "why can he do that" has an answer without a reverse
    /// search through the guild's configuration.
    /// </summary>
    public Guid[] GrantedBy { get; init; } = Array.Empty<Guid>();

    public bool Has(string permissionId) => Permissions.Contains(permissionId);
}
