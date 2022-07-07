namespace TheGuild.Infrastructure.MongoDb.Repositories;

public interface IReadOnlyRepository<TEntity, in TId>
{
    Task<TEntity> GetAsync(TId id);
    Task<ICollection<TEntity>> GetAllAsync();
}