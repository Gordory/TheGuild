using MongoDB.Driver;
using TheGuild.Infrastructure.MongoDb.Collections;

namespace TheGuild.Infrastructure.MongoDb.Repositories;

public abstract class RepositoryBase<TEntity, TId> : IRepositoryBase<TEntity, TId>
{
    protected readonly IMongoDatabase MongoDatabase;

    private readonly IMongoDbCollectionNamesCache _mongoDbCollectionNamesCache;

    protected RepositoryBase(IMongoDatabase mongoDatabase, IMongoDbCollectionNamesCache mongoDbCollectionNamesCache)
    {
        MongoDatabase = mongoDatabase;
        _mongoDbCollectionNamesCache = mongoDbCollectionNamesCache;
    }

    protected virtual int DefaultBatchSize => 100;
    protected virtual ReadConcern ReadConcern => ReadConcern.Default;

    protected virtual IMongoCollection<TEntity> GetCollection(
        ReadPreferenceMode readPreferenceMode,
        WriteConcern? writeConcern = null,
        ReadConcern? readConcern = null)
    {
        return MongoDatabase.GetCollection<TEntity>(
            _mongoDbCollectionNamesCache.Get<TEntity>(),
            new MongoCollectionSettings
            {
                WriteConcern = writeConcern ?? WriteConcern.Unacknowledged,
                ReadConcern = readConcern ?? ReadConcern.Default,
                ReadPreference = new ReadPreference(readPreferenceMode)
            });
    }

    public abstract Task<TEntity> GetAsync(TId id);

    public abstract Task<ICollection<TEntity>> GetAllAsync();

    public abstract Task<TEntity> CreateAsync(TEntity entity);

    public abstract Task<TEntity> UpdateAsync(TEntity entity);

    public abstract Task DeleteAsync(TId id);
}