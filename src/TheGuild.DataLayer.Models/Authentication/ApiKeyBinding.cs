using MongoDB.Bson.Serialization.Attributes;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Entities;

namespace TheGuild.DataLayer.Models.Authentication;

[MongoCollection("Authentication.ApiKeys")]
[BsonKnownTypes(typeof(DiscordBotApiKeyBinding), typeof(TestApiKeyBinding))]
public abstract class ApiKeyBinding : IIdentifiedEntity<Guid>
{
    [BsonId]
    public Guid Id { get; init; }

    public bool Enabled { get; init; }

    public string ServiceName { get; set; }
}