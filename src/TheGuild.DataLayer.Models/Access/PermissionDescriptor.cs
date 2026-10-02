using MongoDB.Bson.Serialization.Attributes;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Entities;

namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// A stored view of one catalogue entry, so the settings screen can be drawn with a query and a
/// label can be fixed without a release. The code catalogue stays the authority: permission checks
/// and grant validation never read this collection, so an edit here cannot break enforcement.
/// </summary>
[MongoCollection("Access.Permissions")]
public class PermissionDescriptor : IIdentifiedEntity<string>
{
    [BsonId]
    public string Id { get; init; } = string.Empty;

    // Projected from code on every start; anything written here by hand is overwritten.
    public string Resource { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string[] Implies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Set when the code no longer declares this id. Kept rather than deleted, because roles may
    /// still carry the grant and an officer needs to see why it stopped doing anything.
    /// </summary>
    public bool Deprecated { get; set; }

    // Presentation, owned by whoever curates the screen. The projection leaves these alone.
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string? Group { get; set; }

    public int? Order { get; set; }
}
