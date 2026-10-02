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

    private const string DatabaseName = "theguild-tests";

    public IMongoDatabase Database { get; private set; } = null!;

    /// <summary>
    /// The Identity store reads the database out of its connection string, so it needs one that names
    /// the database — and an explicit authentication source, since authenticating against the test
    /// database rather than admin would be rejected.
    /// </summary>
    public string IdentityConnectionString { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = _container.GetConnectionString();

        Database = new MongoClient(connectionString).GetDatabase(DatabaseName);

        IdentityConnectionString = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = DatabaseName,
            AuthenticationSource = "admin",
        }.ToString();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
