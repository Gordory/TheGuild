using TheGuild.Api.Authorization;
using TheGuild.Api.Services.Attendance;
using TheGuild.Api.Tests.Fakes;
using TheGuild.DataLayer.Models.Access;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using Xunit;
using ApiAttendanceWarningType = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarningType;

namespace TheGuild.Api.Tests.Services.Attendance;

public class AttendanceWarningServiceTests
{
    private const ulong ServerId = 111;
    private const ulong OtherServerId = 112;
    private const ulong ActorDiscordId = 222;
    private const ulong SomeoneElseDiscordId = 999;

    private static readonly Guid ActorUserId = Guid.NewGuid();
    private static readonly Guid SomeoneElseUserId = Guid.NewGuid();
    private static readonly DateTime Date = new(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Reading_only_own_warnings_narrows_the_query_to_the_actor()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(ActorUserId), Warning(SomeoneElseUserId));
        var service = Service(repository, out _);

        // Asks about somebody else on purpose: the scope has to override what was requested.
        await service.FindAsync(
            Actor(PermissionCatalog.AttendanceWarning.ReadOwn),
            Date,
            SomeoneElseDiscordId);

        Assert.Equal(ActorUserId, repository.LastFind!.Value.UserId);
    }

    [Fact]
    public async Task Reading_own_warnings_without_an_account_returns_nothing_rather_than_everything()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(ActorUserId), Warning(SomeoneElseUserId));
        var service = Service(repository, out _);

        var actor = Actor(PermissionCatalog.AttendanceWarning.ReadOwn) with { UserId = null };

        var found = await service.FindAsync(actor, Date, null);

        // A null account must never reach the repository as "no filter".
        Assert.Empty(found);
        Assert.Null(repository.LastFind);
    }

    [Fact]
    public async Task Reading_every_warning_passes_the_requested_member_through()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(ActorUserId), Warning(SomeoneElseUserId));
        var service = Service(repository, out _);

        await service.FindAsync(
            Actor(PermissionCatalog.AttendanceWarning.ReadAny),
            Date,
            SomeoneElseDiscordId);

        Assert.Equal(SomeoneElseUserId, repository.LastFind!.Value.UserId);
    }

    [Fact]
    public async Task Reading_without_permission_returns_nothing_and_never_queries()
    {
        var repository = new FakeAttendanceWarningRepository(Warning(ActorUserId));
        var service = Service(repository, out _);

        var found = await service.FindAsync(Actor(), Date, null);

        Assert.Empty(found);
        Assert.Null(repository.LastFind);
    }

    [Fact]
    public async Task The_private_comment_is_hidden_without_the_permission_for_it()
    {
        var warning = Warning(ActorUserId);
        var service = Service(new FakeAttendanceWarningRepository(warning), out _);

        var found = await service.GetAsync(Actor(PermissionCatalog.AttendanceWarning.ReadAny), warning.Id);

        Assert.Equal("Public", found!.PublicComment);
        Assert.Null(found.PrivateComment);
    }

    [Fact]
    public async Task The_private_comment_shows_with_the_permission_for_it()
    {
        var warning = Warning(ActorUserId);
        var service = Service(new FakeAttendanceWarningRepository(warning), out _);

        var actor = Actor(
            PermissionCatalog.AttendanceWarning.ReadAny,
            PermissionCatalog.AttendanceWarning.ReadPrivate);

        var found = await service.GetAsync(actor, warning.Id);

        Assert.Equal("Private", found!.PrivateComment);
    }

    [Fact]
    public async Task A_warning_of_another_guild_is_not_found()
    {
        var warning = Warning(ActorUserId);
        var service = Service(new FakeAttendanceWarningRepository(warning), out _);

        var actor = Actor(PermissionCatalog.AttendanceWarning.ReadAny) with { DiscordServerId = OtherServerId };

        Assert.Null(await service.GetAsync(actor, warning.Id));
    }

    [Fact]
    public async Task Editing_someone_elses_warning_needs_the_broader_permission()
    {
        var warning = Warning(SomeoneElseUserId);
        var service = Service(new FakeAttendanceWarningRepository(warning), out _);

        var denied = await Assert.ThrowsAsync<GuildAccessDeniedException>(() =>
            service.UpdateAsync(
                Actor(PermissionCatalog.AttendanceWarning.UpdateOwn),
                warning.Id,
                UpdateRequest()));

        Assert.Equal(PermissionCatalog.AttendanceWarning.UpdateAny, denied.MissingPermissionId);
    }

    [Fact]
    public async Task Editing_your_own_warning_needs_only_the_narrower_permission()
    {
        var warning = Warning(ActorUserId);
        var service = Service(new FakeAttendanceWarningRepository(warning), out _);

        var updated = await service.UpdateAsync(
            Actor(PermissionCatalog.AttendanceWarning.UpdateOwn),
            warning.Id,
            UpdateRequest());

        Assert.NotNull(updated);
        Assert.Equal("Edited", updated!.PublicComment);
    }

    [Fact]
    public async Task A_refused_recording_leaves_nobody_with_a_new_account()
    {
        var service = Service(new FakeAttendanceWarningRepository(), out var accounts);

        var denied = await Assert.ThrowsAsync<GuildAccessDeniedException>(() =>
            service.CreateAsync(
                Actor(PermissionCatalog.AttendanceWarning.CreateOwn),
                CreateRequest(SomeoneElseDiscordId)));

        Assert.Equal(PermissionCatalog.AttendanceWarning.CreateAny, denied.MissingPermissionId);
        Assert.Equal(0, accounts.Created);
    }

    [Fact]
    public async Task Recording_a_warning_gives_an_account_to_a_member_who_never_signed_in()
    {
        var service = Service(new FakeAttendanceWarningRepository(), out var accounts);

        // 404040 is nobody the resolver knows, which is the point: an officer must be able to record
        // a no-show for somebody who has never opened the site.
        var created = await service.CreateAsync(
            Actor(PermissionCatalog.AttendanceWarning.CreateAny),
            CreateRequest(404040));

        Assert.Equal(1, accounts.Created);
        Assert.NotEqual(Guid.Empty, created.UserId);
    }

    [Fact]
    public async Task A_recorded_warning_is_dated_by_day_so_the_lookup_can_find_it()
    {
        var service = Service(new FakeAttendanceWarningRepository(), out _);

        var created = await service.CreateAsync(
            Actor(PermissionCatalog.AttendanceWarning.CreateAny),
            CreateRequest(SomeoneElseDiscordId) with { DateStart = Date.AddHours(13).AddMinutes(45) });

        Assert.Equal(Date, created.DateStart);
    }

    private static AttendanceWarningService Service(
        FakeAttendanceWarningRepository repository,
        out FakeGuildMemberAccountResolver accounts)
    {
        accounts = new FakeGuildMemberAccountResolver(
            (ActorDiscordId, ActorUserId),
            (SomeoneElseDiscordId, SomeoneElseUserId));

        return new AttendanceWarningService(repository, new GuildAuthorizer(), accounts);
    }

    private static GuildActor Actor(params string[] permissions)
    {
        return new GuildActor
        {
            DiscordServerId = ServerId,
            DiscordUserId = ActorDiscordId,
            UserId = ActorUserId,
            Permissions = PermissionCatalog.Expand(permissions),
        };
    }

    private static AttendanceWarning Warning(Guid userId, ulong discordServerId = ServerId)
    {
        return new AttendanceWarning
        {
            Id = Guid.NewGuid(),
            DiscordServerId = discordServerId,
            UserId = userId,
            Type = AttendanceWarningType.Late,
            DateStart = Date,
            PublicComment = "Public",
            PrivateComment = "Private",
            Created = DateTime.UtcNow,
        };
    }

    private static AttendanceWarningCreateRequest CreateRequest(ulong discordUserId)
    {
        return new AttendanceWarningCreateRequest
        {
            DiscordUserId = discordUserId,
            Type = ApiAttendanceWarningType.Late,
            DateStart = Date,
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
