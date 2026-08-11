using MongoDB.Driver;
using TheGuild.DataLayer.Models.Guilds;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Repositories;

namespace TheGuild.DataLayer.Guilds;

public interface IGuildRepository : IRepositoryBase<Guild, Guid>
{
    public Task<Guild?> FindByDiscordServerId(ulong discordServerId);
}

public class GuildRepository : RepositoryBase<Guild, Guid>, IGuildRepository
{
    public GuildRepository(IMongoDatabase mongoDatabase, IMongoDbCollectionNamesCache mongoDbCollectionNamesCache) 
        : base(mongoDatabase, mongoDbCollectionNamesCache)
    {
    }

    public async Task<Guild?> FindByDiscordServerId(ulong discordServerId)
    {
        var filter = Builders<Guild>.Filter.Eq(x => x.DiscordServerId, discordServerId);

        var collection = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(filter);

        return await collection.FirstOrDefaultAsync();
    }
}