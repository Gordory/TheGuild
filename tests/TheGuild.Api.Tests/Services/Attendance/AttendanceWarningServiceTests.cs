using TheGuild.Api.Authorization;
using TheGuild.Api.Services.Attendance;
using TheGuild.Api.Tests.Fakes;
using TheGuild.DataLayer.Models.Access;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using ApiAttendanceWarningType = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarningType;
using Xunit;

namespace TheGuild.Api.Tests.Services.Attendance;

public class AttendanceWarningServiceTests
{
    private const ulong ServerId = 111;
    private const ulong OtherServerId = 112;
    private const ulong MemberId = 222;
    private const ulong SomeoneElseId = 999;

    private static readonly DateTime Date = new(2026, 9, 29, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Reading_only_own_warnings_narrows_the_query_to_the_actor()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(MemberId), Warning(SomeoneElseId));
        var service = Service(repository);

        // Asks about somebody else on purpose: the scope has to override what was requested.
        await service.FindAsync(Actor(PermissionCatalog.AttendanceWarning.ReadOwn), Date, SomeoneElseId);

        Assert.Equal(MemberId, repository.LastFind!.Value.DiscordUserId);
    }

    [Fact]
    public async Task Reading_every_warning_passes_the_requested_member_through()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(MemberId), Warning(SomeoneElseId));
        var service = Service(repository);

        await service.FindAsync(Actor(PermissionCatalog.AttendanceWarning.ReadAny), Date, SomeoneElseId);

        Assert.Equal(SomeoneElseId, repository.LastFind!.Value.DiscordUserId);
    }

    [Fact]
    public async Task Reading_without_permission_returns_nothing_and_never_queries()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(MemberId));
        var service = Service(repository);

        var found = await service.FindAsync(Actor(), Date, null);

        Assert.Empty(found);
        Assert.Null(repository.LastFind);
    }

    [Fact]
    public async Task The_private_comment_is_hidden_without_the_permission_for_it()
    {
        var warning = Warning(MemberId);
        var service = Service(new FakeAttendanceWarningRepository(warning));

        var found = await service.GetAsync(Actor(PermissionCatalog.AttendanceWarning.ReadAny), warning.Id);

        Assert.Equal("Public", found!.PublicComment);
        Assert.Null(found.PrivateComment);
    }

    [Fact]
    public async Task The_private_comment_shows_with_the_permission_for_it()
    {
        var warning = Warning(MemberId);
        var service = Service(new FakeAttendanceWarningRepository(warning));

        var actor = Actor(
            PermissionCatalog.AttendanceWarning.ReadAny,
            PermissionCatalog.AttendanceWarning.ReadPrivate);

        var found = await service.GetAsync(actor, warning.Id);

        Assert.Equal("Private", found!.PrivateComment);
    }

    [Fact]
    public async Task A_warning_of_another_guild_is_not_found()
    {
        var warning = Warning(MemberId, ServerId);
        var service = Service(new FakeAttendanceWarningRepository(warning));

        var actor = Actor(PermissionCatalog.AttendanceWarning.ReadAny) with { DiscordServerId = OtherServerId };

        Assert.Null(await service.GetAsync(actor, warning.Id));
    }

    [Fact]
    public async Task Editing_someone_elses_warning_needs_the_broader_permission()
    {
        var warning = Warning(SomeoneElseId);
        var service = Service(new FakeAttendanceWarningRepository(warning));

        var denied = await Assert.ThrowsAsync<GuildAccessDeniedException>(() =>
            service.UpdateAsync(Actor(PermissionCatalog.AttendanceWarning.UpdateOwn), warning.Id, UpdateRequest()));

        Assert.Equal(PermissionCatalog.AttendanceWarning.UpdateAny, denied.MissingPermissionId);
    }

    [Fact]
    public async Task Editing_your_own_warning_needs_only_the_narrower_permission()
    {
        var warning = Warning(MemberId);
        var service = Service(new FakeAttendanceWarningRepository(warning));

        var updated = await service.UpdateAsync(
            Actor(PermissionCatalog.AttendanceWarning.UpdateOwn),
            warning.Id,
            UpdateRequest());

        Assert.NotNull(updated);
        Assert.Equal("Edited", updated!.PublicComment);
    }

    [Fact]
    public async Task Recording_a_warning_against_someone_else_needs_the_broader_permission()
    {
        var service = Service(new FakeAttendanceWarningRepository());

        var denied = await Assert.ThrowsAsync<GuildAccessDeniedException>(() =>
            service.CreateAsync(
                Actor(PermissionCatalog.AttendanceWarning.CreateOwn),
                new AttendanceWarningCreateRequest { DiscordUserId = SomeoneElseId, DateStart = Date }));

        Assert.Equal(PermissionCatalog.AttendanceWarning.CreateAny, denied.MissingPermissionId);
    }

    [Fact]
    public async Task A_recorded_warning_is_dated_by_day_so_the_lookup_can_find_it()
    {
        var repository = new FakeAttendanceWarningRepository();
        var service = Service(repository);

        var created = await service.CreateAsync(
            Actor(PermissionCatalog.AttendanceWarning.CreateAny),
            new AttendanceWarningCreateRequest
            {
                DiscordUserId = SomeoneElseId,
                DateStart = Date.AddHours(13).AddMinutes(45),
            });

        Assert.Equal(Date, created.DateStart);
    }

    private static AttendanceWarningService Service(FakeAttendanceWarningRepository repository)
    {
        return new AttendanceWarningService(repository, new GuildAuthorizer());
    }

    private static GuildActor Actor(params string[] permissions)
    {
        return new GuildActor
        {
            DiscordServerId = ServerId,
            DiscordUserId = MemberId,
            Permissions = PermissionCatalog.Expand(permissions),
        };
    }

    private static AttendanceWarning Warning(ulong discordUserId, ulong discordServerId = ServerId)
    {
        return new AttendanceWarning
        {
            Id = Guid.NewGuid(),
            DiscordServerId = discordServerId,
            DiscordUserId = discordUserId,
            Type = AttendanceWarningType.Late,
            DateStart = Date,
            PublicComment = "Public",
            PrivateComment = "Private",
            Created = DateTime.UtcNow,
        };
    }

    private static AttendanceWarningUpdateRequest UpdateRequest()
    {
        return new AttendanceWarningUpdateRequest
        {
            Type = ApiAttendanceWarningType.Absence,
            DateStart = Date,
            PublicComment = "Edited",
        };
    }
}
