using MongoDB.Driver;
using TheGuild.DataLayer.Models.Access;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Repositories;

namespace TheGuild.DataLayer.Access;

public interface IPermissionDescriptorRepository : IRepositoryBase<PermissionDescriptor, string>
{
    Task ProjectAsync(IReadOnlyCollection<Permission> declared);
}

public class PermissionDescriptorRepository : RepositoryBase<PermissionDescriptor, string>, IPermissionDescriptorRepository
{
    public PermissionDescriptorRepository(
        IMongoDatabase mongoDatabase,
        IMongoDbCollectionNamesCache mongoDbCollectionNamesCache)
        : base(mongoDatabase, mongoDbCollectionNamesCache)
    {
    }

    /// <summary>
    /// Brings the collection in line with the code catalogue. Updates named fields rather than
    /// replacing documents, because the presentation fields belong to whoever edited them.
    /// </summary>
    public async Task ProjectAsync(IReadOnlyCollection<Permission> declared)
    {
        var collection = GetCollection(ReadPreferenceMode.Primary, WriteConcern.WMajority);

        var declaredIds = declared.Select(permission => permission.Id).ToArray();

        var upserts = declared
            .Select(permission => new UpdateOneModel<PermissionDescriptor>(
                Builders<PermissionDescriptor>.Filter.Eq(descriptor => descriptor.Id, permission.Id),
                Builders<PermissionDescriptor>.Update
                    .Set(descriptor => descriptor.Resource, permission.Resource)
                    .Set(descriptor => descriptor.Action, permission.Action)
                    .Set(descriptor => descriptor.Implies, permission.Implies)
                    .Set(descriptor => descriptor.Deprecated, false))
            {
                IsUpsert = true,
            })
            .ToList<WriteModel<PermissionDescriptor>>();

        if (upserts.Count > 0)
        {
            await collection.BulkWriteAsync(upserts);
        }

        await collection.UpdateManyAsync(
            Builders<PermissionDescriptor>.Filter.Nin(descriptor => descriptor.Id, declaredIds),
            Builders<PermissionDescriptor>.Update.Set(descriptor => descriptor.Deprecated, true));
    }
}
