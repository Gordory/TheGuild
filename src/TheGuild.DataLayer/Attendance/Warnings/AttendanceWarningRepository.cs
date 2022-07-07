using MongoDB.Driver;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Repositories;

namespace TheGuild.DataLayer.Attendance.Warnings;

public interface IAttendanceWarningRepository : ICrudRepository<AttendanceWarning, Guid>
{
    Task<ICollection<AttendanceWarning>> Find(DateTime dateTime, ulong? discordUserId = null);
}

public class AttendanceWarningRepository : RepositoryBase<AttendanceWarning, Guid>, IAttendanceWarningRepository
{
    public AttendanceWarningRepository(
        IMongoDatabase mongoDatabase,
        IMongoDbCollectionNamesCache mongoDbCollectionNamesCache) 
        : base(mongoDatabase, mongoDbCollectionNamesCache)
    {
    }

    public override async Task<AttendanceWarning> GetAsync(Guid id)
    {
        var cursor = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(FilterId(id));

        return await cursor.FirstOrDefaultAsync() 
               ?? throw new KeyNotFoundException($"AttendanceWarning with id was not found: {id}");
    }

    public override async Task<ICollection<AttendanceWarning>> GetAllAsync()
    {
        var cursor = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(Builders<AttendanceWarning>.Filter.Empty);

        return await cursor.ToListAsync();
    }

    public async Task<ICollection<AttendanceWarning>> Find(DateTime dateTime, ulong? discordUserId = null)
    {
        var utcDate = dateTime.ToUniversalTime().Date;

        var filter = Builders<AttendanceWarning>.Filter.Or(
            Builders<AttendanceWarning>.Filter.And(
                Builders<AttendanceWarning>.Filter.Eq(x => x.DateStart, utcDate),
                Builders<AttendanceWarning>.Filter.Eq(x => x.DateEnd, null)),
            Builders<AttendanceWarning>.Filter.And(
                Builders<AttendanceWarning>.Filter.Lte(x => x.DateStart, utcDate),
                Builders<AttendanceWarning>.Filter.Gte(x => x.DateEnd, utcDate)));

        if (discordUserId != null)
            filter = Builders<AttendanceWarning>.Filter.And(
                filter,
                Builders<AttendanceWarning>.Filter.Eq(x => x.DiscordUserId, discordUserId));

        var collection = await GetCollection(ReadPreferenceMode.SecondaryPreferred, readConcern: ReadConcern)
            .FindAsync(filter);

        return await collection.ToListAsync();
    }

    public override async Task<AttendanceWarning> CreateAsync(AttendanceWarning entity)
    {
        await GetCollection(ReadPreferenceMode.Primary, WriteConcern.WMajority)
            .InsertOneAsync(entity);

        return entity;
    }

    public override async Task<AttendanceWarning> UpdateAsync(AttendanceWarning entity)
    {
        await GetCollection(ReadPreferenceMode.Primary, WriteConcern.WMajority)
            .ReplaceOneAsync(FilterId(entity.Id), entity);

        return entity;
    }

    public override async Task DeleteAsync(Guid id)
    {
        await GetCollection(ReadPreferenceMode.PrimaryPreferred, WriteConcern.WMajority)
            .DeleteOneAsync(FilterId(id));
    }

    private static FilterDefinition<AttendanceWarning> FilterId(Guid id)
    {
        return Builders<AttendanceWarning>.Filter.Eq(x => x.Id, id);
    }
}