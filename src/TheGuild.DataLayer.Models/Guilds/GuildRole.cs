namespace TheGuild.DataLayer.Models.Guilds;

/// <summary>
/// A role of this guild, and the only thing that grants permissions. Membership is not stored
/// wholesale: it follows from the Discord roles linked here, so Discord remains the single answer to
/// who is a raid leader, and a member's every permission is still explained by a named role.
/// </summary>
public sealed class GuildRole
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public ulong[] DiscordRoleIds { get; init; } = Array.Empty<ulong>();

    /// <summary>
    /// Members named outright, by account. Also how a role with no Discord counterpart works.
    /// </summary>
    public Guid[] UserIds { get; init; } = Array.Empty<Guid>();

    public PermissionGrant[] Grants { get; init; } = Array.Empty<PermissionGrant>();
}
