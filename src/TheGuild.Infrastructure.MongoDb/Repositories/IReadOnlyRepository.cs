namespace TheGuild.Infrastructure.MongoDb.Repositories;

public interface IReadOnlyRepository<TEntity, in TId>
{
    Task<TEntity?> FindAsync(TId id);
    Task<ICollection<TEntity>> GetAllAsync();
}