using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace TheGuild.Infrastructure.MongoDb;

internal static class MongoDbSerialization
{
    /// <summary>
    /// Driver 3 dropped the legacy Guid representation modes, so leaving the choice implicit would
    /// make the stored byte layout of every Guid _id a property of driver defaults rather than of
    /// this codebase. Standard is the UUID binary subtype that every other MongoDB driver reads.
    /// A module initializer rather than a call from startup, because the repositories are also
    /// constructed directly by the tests, and a registration someone has to remember to make is a
    /// registration someone will forget.
    /// </summary>
    [ModuleInitializer]
    internal static void Configure()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
