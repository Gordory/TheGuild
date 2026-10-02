using MongoDB.Bson.Serialization.Attributes;

namespace TheGuild.Infrastructure.MongoDb.Entities;

public interface IIdentifiedEntity<TId>
{
    public TId Id { get; init; }
}