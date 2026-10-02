using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace TheGuild.DataLayer.Tests;

/// <summary>
/// Runs the repository tests against a real MongoDB: the isolation these tests guard lives in
/// the driver's filter translation, which an in-memory fake would not reproduce.
/// </summary>
public class MongoContainerFixture : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:6.0").Build();

    public IMongoDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        Database = new MongoClient(_container.GetConnectionString()).GetDatabase("theguild-tests");
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
