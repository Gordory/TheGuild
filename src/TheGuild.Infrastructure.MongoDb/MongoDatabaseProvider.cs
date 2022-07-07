using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheGuild.Infrastructure.MongoDb.Configuration;

namespace TheGuild.Infrastructure.MongoDb;

public interface IMongoDatabaseProvider
{
    IMongoDatabase Get();
}

public class MongoDatabaseProvider : IMongoDatabaseProvider 
{
    private readonly IMongoClientProvider _mongoClientProvider;
    private readonly IOptions<MongoDbConfiguration> _mongoDbConfiguration;

    public MongoDatabaseProvider(IMongoClientProvider mongoClientProvider, IOptions<MongoDbConfiguration> mongoDbConfiguration)
    {
        _mongoClientProvider = mongoClientProvider;
        _mongoDbConfiguration = mongoDbConfiguration;
    }
    
    public IMongoDatabase Get()
    {
        return _mongoClientProvider.Get().GetDatabase(_mongoDbConfiguration.Value.Database);
    }
}