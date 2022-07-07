namespace TheGuild.Infrastructure.MongoDb.Repositories;

public interface ICrudRepository<TEntity, in TId> : IReadOnlyRepository<TEntity, TId>
{
    Task<TEntity> CreateAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(TId id);
}