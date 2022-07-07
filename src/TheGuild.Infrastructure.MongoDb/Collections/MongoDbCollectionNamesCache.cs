using System.Collections.Concurrent;

namespace TheGuild.Infrastructure.MongoDb.Collections;

public interface IMongoDbCollectionNamesCache
{
    string Get<T>();

    string Get(Type key);
}

public class MongoDbCollectionNamesCache : IMongoDbCollectionNamesCache
{
    private readonly ConcurrentDictionary<Type, string> _cachedCollectionNames = new();

    public string GetOrAdd(Type key, Func<Type, string> valueFactory) =>
        _cachedCollectionNames.GetOrAdd(key, valueFactory);

    public string Get<T>()
    {
        return Get(typeof(T));
    }

    public string Get(Type key)
    {
        if (!_cachedCollectionNames.TryGetValue(key, out var collectionName))
        {
            collectionName = GetCollectionNameByType(key);
        }

        return collectionName;
    }

    private static string GetCollectionNameByType(Type key)
    {
        return key
            .GetCustomAttributes(false)
            .OfType<MongoCollectionAttribute>()
            .FirstOrDefault()?
            .Name ?? throw new InvalidOperationException($"Mongo collection name not specified: {key}");
    }
}