using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TheGuild.Infrastructure.MongoDb.Configuration;

namespace TheGuild.Infrastructure.MongoDb;

public interface IMongoClientProvider
{
    IMongoClient Get();
}

public class MongoClientProvider : IMongoClientProvider
{
    private readonly IOptions<MongoDbConfiguration> _mongoDbConfiguration;

    public MongoClientProvider(IOptions<MongoDbConfiguration> mongoDbConfiguration)
    {
        _mongoDbConfiguration = mongoDbConfiguration;
    }

    public IMongoClient Get()
    {
        return new MongoClient(_mongoDbConfiguration.Value.ConnectionString);
    }
}