namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// The two permissions of one verb, separated by whose record is being touched. The authorizer
/// picks between them from the resource at hand, so the distinction stays out of the call sites.
/// </summary>
public sealed record PermissionPair(string Own, string Any);
