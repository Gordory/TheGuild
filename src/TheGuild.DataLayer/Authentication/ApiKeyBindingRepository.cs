using MongoDB.Driver;
using TheGuild.DataLayer.Models.Authentication;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Repositories;

namespace TheGuild.DataLayer.Authentication;

public interface IApiKeyBindingRepository : IRepositoryBase<ApiKeyBinding, Guid>
{
}

public class ApiKeyBindingRepository : RepositoryBase<ApiKeyBinding, Guid>, IApiKeyBindingRepository
{
    public ApiKeyBindingRepository(IMongoDatabase mongoDatabase, IMongoDbCollectionNamesCache mongoDbCollectionNamesCache) : base(mongoDatabase, mongoDbCollectionNamesCache)
    {
    }
}