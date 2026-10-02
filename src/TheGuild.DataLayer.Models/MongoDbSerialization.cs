using System.Runtime.CompilerServices;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace TheGuild.DataLayer.Models;

internal static class MongoDbSerialization
{
    /// <summary>
    /// Driver 3 dropped the legacy Guid representation modes and leaves the default Unspecified,
    /// which refuses to serialize a Guid at all. Standard is the UUID binary subtype every other
    /// MongoDB driver reads.
    /// This lives in the assembly that declares the documents, so the trigger is self-enforcing: no
    /// entity of ours can be serialized without first loading the assembly that defines it. Placing
    /// it next to the Mongo infrastructure looked tidier and silently did nothing, because a caller
    /// using only the Identity entities never loaded that assembly.
    /// Register rather than TryRegister: a conflicting registration means the stored byte layout is
    /// not what this file says, and that should fail loudly rather than pass unnoticed.
    /// </summary>
    [ModuleInitializer]
    internal static void Configure()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
    }
}
