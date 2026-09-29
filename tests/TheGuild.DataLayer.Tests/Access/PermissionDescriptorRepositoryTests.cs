using MongoDB.Driver;
using TheGuild.DataLayer.Access;
using TheGuild.DataLayer.Models.Access;
using TheGuild.Infrastructure.MongoDb.Collections;
using Xunit;

namespace TheGuild.DataLayer.Tests.Access;

public class PermissionDescriptorRepositoryTests : IClassFixture<MongoContainerFixture>, IAsyncLifetime
{
    private const string Declared = "attendance.warning.read.own";
    private const string Withdrawn = "raid.signup.manage";

    private readonly MongoContainerFixture _mongo;
    private readonly PermissionDescriptorRepository _repository;

    public PermissionDescriptorRepositoryTests(MongoContainerFixture mongo)
    {
        _mongo = mongo;
        _repository = new PermissionDescriptorRepository(mongo.Database, new MongoDbCollectionNamesCache());
    }

    public async Task InitializeAsync()
    {
        await _mongo.Database.DropCollectionAsync("Access.Permissions");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Projecting_stores_every_declared_permission()
    {
        await _repository.ProjectAsync(PermissionCatalog.All);

        var stored = await _repository.GetAllAsync();

        Assert.Equal(
            PermissionCatalog.All.Select(permission => permission.Id).OrderBy(id => id),
            stored.Select(descriptor => descriptor.Id).OrderBy(id => id));
    }

    [Fact]
    public async Task Projecting_again_keeps_an_edited_label()
    {
        await _repository.ProjectAsync(PermissionCatalog.All);
        await SetDisplayNameAsync(Declared, "Own warnings");

        await _repository.ProjectAsync(PermissionCatalog.All);

        var descriptor = await _repository.FindAsync(Declared);
        Assert.Equal("Own warnings", descriptor!.DisplayName);
    }

    [Fact]
    public async Task A_permission_the_catalogue_dropped_is_deprecated_rather_than_removed()
    {
        await _repository.ProjectAsync(new[] { new Permission(Withdrawn, "raid.signup", "manage") });

        await _repository.ProjectAsync(PermissionCatalog.All);

        var descriptor = await _repository.FindAsync(Withdrawn);
        Assert.NotNull(descriptor);
        Assert.True(descriptor!.Deprecated);
    }

    [Fact]
    public async Task Declaring_a_permission_again_clears_its_deprecation()
    {
        await _repository.ProjectAsync(new[] { new Permission(Withdrawn, "raid.signup", "manage") });
        await _repository.ProjectAsync(PermissionCatalog.All);

        await _repository.ProjectAsync(new[] { new Permission(Withdrawn, "raid.signup", "manage") });

        var descriptor = await _repository.FindAsync(Withdrawn);
        Assert.False(descriptor!.Deprecated);
    }

    private Task SetDisplayNameAsync(string permissionId, string displayName)
    {
        return _mongo.Database
            .GetCollection<PermissionDescriptor>("Access.Permissions")
            .UpdateOneAsync(
                Builders<PermissionDescriptor>.Filter.Eq(descriptor => descriptor.Id, permissionId),
                Builders<PermissionDescriptor>.Update.Set(descriptor => descriptor.DisplayName, displayName));
    }
}
