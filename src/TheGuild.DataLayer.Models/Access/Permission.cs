namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// One atomic capability an officer can grant. A permission exists only because some code checks
/// for it, which is why the catalogue lives in code rather than in the database.
/// </summary>
public sealed record Permission(string Id, string Resource, string Action)
{
    /// <summary>
    /// Permissions this one subsumes. Replaces the composite flag members of the old enum: holding
    /// <c>read.any</c> means holding <c>read.own</c>, without either value being written by hand.
    /// </summary>
    public string[] Implies { get; init; } = Array.Empty<string>();
}
