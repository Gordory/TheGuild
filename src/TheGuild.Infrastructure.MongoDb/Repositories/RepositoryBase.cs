using MongoDB.Driver;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Entities;

namespace TheGuild.Infrastructure.MongoDb.Repositories;

public abstract class RepositoryBase<TEntity, TId> : IRepositoryBase<TEntity, TId>
    where TEntity : IIdentifiedEntity<TId>
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

    public virtual async Task<TEntity?> FindAsync(TId id)
    {
        var cursor = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(FilterId(id));

        return await cursor.FirstOrDefaultAsync();
    }

    public virtual async Task<ICollection<TEntity>> GetAllAsync()
    {
        var cursor = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(Builders<TEntity>.Filter.Empty);

        return await cursor.ToListAsync();
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        await GetCollection(ReadPreferenceMode.Primary, WriteConcern.WMajority)
            .InsertOneAsync(entity);

        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        await GetCollection(ReadPreferenceMode.Primary, WriteConcern.WMajority)
            .ReplaceOneAsync(FilterId(entity.Id), entity);

        return entity;
    }

    public virtual async Task DeleteAsync(TId id) 
    {
        await GetCollection(ReadPreferenceMode.PrimaryPreferred, WriteConcern.WMajority)
            .DeleteOneAsync(FilterId(id));
    }

    protected FilterDefinition<TEntity> FilterId(TId id)
    {
        return Builders<TEntity>.Filter.Eq(x => x.Id, id);
    }
}