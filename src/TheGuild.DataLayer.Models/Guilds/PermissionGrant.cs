namespace TheGuild.DataLayer.Models.Guilds;

/// <summary>
/// One permission attached to a role. A document rather than a bare string so that parameterised
/// permissions -- "edit own within N hours" -- arrive as a field instead of a migration over every
/// guild's embedded arrays.
/// </summary>
public sealed record PermissionGrant(string PermissionId);
