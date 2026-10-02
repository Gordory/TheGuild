using MongoDB.Driver;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Repositories;

namespace TheGuild.DataLayer.Attendance.Warnings;

public interface IAttendanceWarningRepository : IRepositoryBase<AttendanceWarning, Guid>
{
    Task<ICollection<AttendanceWarning>> Find(ulong discordServerId, DateTime dateTime, Guid? userId = null);

    /// <summary>
    /// Guild-scoped counterpart of <see cref="IReadOnlyRepository{TEntity,TId}.FindAsync"/>: the id alone
    /// is guessable across guilds, so callers serving a request must go through this overload.
    /// </summary>
    Task<AttendanceWarning?> FindAsync(ulong discordServerId, Guid id);
}

public class AttendanceWarningRepository : RepositoryBase<AttendanceWarning, Guid>, IAttendanceWarningRepository
{
    public AttendanceWarningRepository(
        IMongoDatabase mongoDatabase,
        IMongoDbCollectionNamesCache mongoDbCollectionNamesCache) 
        : base(mongoDatabase, mongoDbCollectionNamesCache)
    {
    }

    public async Task<ICollection<AttendanceWarning>> Find(ulong discordServerId, DateTime dateTime, Guid? userId = null)
    {
        var utcDate = dateTime.ToUniversalTime().Date;

        var filter = Builders<AttendanceWarning>.Filter.And(
            FilterDiscordServerId(discordServerId),
            Builders<AttendanceWarning>.Filter.Or(
                Builders<AttendanceWarning>.Filter.And(
                    Builders<AttendanceWarning>.Filter.Eq(x => x.DateStart, utcDate),
                    Builders<AttendanceWarning>.Filter.Eq(x => x.DateEnd, null)),
                Builders<AttendanceWarning>.Filter.And(
                    Builders<AttendanceWarning>.Filter.Lte(x => x.DateStart, utcDate),
                    Builders<AttendanceWarning>.Filter.Gte(x => x.DateEnd, utcDate))));

        if (userId != null)
            filter = Builders<AttendanceWarning>.Filter.And(
                filter,
                Builders<AttendanceWarning>.Filter.Eq(x => x.UserId, userId));

        var collection = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(filter);

        return await collection.ToListAsync();
    }

    public async Task<AttendanceWarning?> FindAsync(ulong discordServerId, Guid id)
    {
        var filter = Builders<AttendanceWarning>.Filter.And(
            FilterId(id),
            FilterDiscordServerId(discordServerId));

        var cursor = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(filter);

        return await cursor.FirstOrDefaultAsync();
    }

    private static FilterDefinition<AttendanceWarning> FilterDiscordServerId(ulong discordServerId)
    {
        return Builders<AttendanceWarning>.Filter.Eq(x => x.DiscordServerId, discordServerId);
    }
}