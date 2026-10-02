using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using TheGuild.Infrastructure.MongoDb.Collections;
using Xunit;

namespace TheGuild.DataLayer.Tests.Attendance.Warnings;

public class AttendanceWarningRepositoryTests : IClassFixture<MongoContainerFixture>, IAsyncLifetime
{
    private const ulong OwnGuild = 111;
    private const ulong OtherGuild = 222;
    private static readonly Guid Member = Guid.NewGuid();

    private readonly MongoContainerFixture _mongo;
    private readonly AttendanceWarningRepository _repository;

    public AttendanceWarningRepositoryTests(MongoContainerFixture mongo)
    {
        _mongo = mongo;
        _repository = new AttendanceWarningRepository(mongo.Database, new MongoDbCollectionNamesCache());
    }

    public async Task InitializeAsync()
    {
        await _mongo.Database.DropCollectionAsync("AttendanceWarnings");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Find_does_not_return_warnings_of_another_guild()
    {
        var date = DateTime.UtcNow.Date;
        await _repository.CreateAsync(Warning(OwnGuild, date));
        await _repository.CreateAsync(Warning(OtherGuild, date));

        var found = await _repository.Find(OwnGuild, date, Member);

        var warning = Assert.Single(found);
        Assert.Equal(OwnGuild, warning.DiscordServerId);
    }

    [Fact]
    public async Task FindAsync_does_not_return_a_warning_of_another_guild()
    {
        var date = DateTime.UtcNow.Date;
        var foreignWarning = await _repository.CreateAsync(Warning(OtherGuild, date));

        var found = await _repository.FindAsync(OwnGuild, foreignWarning.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task FindAsync_returns_a_warning_of_the_requested_guild()
    {
        var date = DateTime.UtcNow.Date;
        var ownWarning = await _repository.CreateAsync(Warning(OwnGuild, date));

        var found = await _repository.FindAsync(OwnGuild, ownWarning.Id);

        Assert.NotNull(found);
        Assert.Equal(ownWarning.Id, found!.Id);
    }

    private static AttendanceWarning Warning(ulong discordServerId, DateTime dateStart)
    {
        return new AttendanceWarning
        {
            Id = Guid.NewGuid(),
            DiscordServerId = discordServerId,
            UserId = Member,
            Type = AttendanceWarningType.Late,
            DateStart = dateStart,
            PublicComment = "Public",
            PrivateComment = "Private",
            Created = DateTime.UtcNow,
        };
    }
}
