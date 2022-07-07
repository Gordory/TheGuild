namespace TheGuild.Infrastructure.MongoDb.Repositories;

public interface IRepositoryBase<TEntity, TId> : ICrudRepository<TEntity, TId>, IReadOnlyRepository<TEntity, TId>
{
}