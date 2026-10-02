using MongoDB.Bson.Serialization.Attributes;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Entities;

namespace TheGuild.DataLayer.Models.Guilds;

[MongoCollection("Guilds")]
public class Guild : IIdentifiedEntity<Guid>
{
    [BsonId]
    public Guid Id { get; init; }

    public string? UniqueName { get; init; }

    public ulong DiscordServerId { get; init; }

    public string? Name { get; init; }

    public Guid OwnerUserId { get; init; }

    // A guild with no roles configured yet is normal; permission resolution reads this eagerly.
    public GuildRole[] GuildRoles { get; init; } = Array.Empty<GuildRole>();
}