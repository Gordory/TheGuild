using TheGuild.Api.Authorization;
using TheGuild.DataLayer.Models.Access;
using Xunit;

namespace TheGuild.Api.Tests.Authorization;

public class GuildAuthorizerTests
{
    private const ulong MemberId = 222;
    private const ulong SomeoneElseId = 999;

    private static readonly PermissionPair Update = PermissionCatalog.AttendanceWarning.Update;

    private readonly GuildAuthorizer _authorizer = new();

    [Fact]
    public void The_broader_permission_covers_someone_elses_record()
    {
        var allowed = _authorizer.IsAllowed(Actor(Update.Any), Update, Owned(SomeoneElseId));

        Assert.True(allowed);
    }

    [Fact]
    public void The_narrower_permission_covers_the_actors_own_record()
    {
        var allowed = _authorizer.IsAllowed(Actor(Update.Own), Update, Owned(MemberId));

        Assert.True(allowed);
    }

    [Fact]
    public void The_narrower_permission_does_not_reach_someone_elses_record()
    {
        var allowed = _authorizer.IsAllowed(Actor(Update.Own), Update, Owned(SomeoneElseId));

        Assert.False(allowed);
    }

    [Fact]
    public void Without_a_resource_the_narrower_permission_is_not_enough()
    {
        // Nothing has pinned the request to the actor's own record, so "own" cannot answer it.
        var allowed = _authorizer.IsAllowed(Actor(Update.Own), Update);

        Assert.False(allowed);
    }

    [Fact]
    public void Without_a_resource_the_broader_permission_is_enough()
    {
        Assert.True(_authorizer.IsAllowed(Actor(Update.Any), Update));
    }

    [Fact]
    public void Holding_neither_permission_allows_nothing()
    {
        var actor = Actor(PermissionCatalog.AttendanceWarning.ReadAny);

        Assert.False(_authorizer.IsAllowed(actor, Update, Owned(MemberId)));
        Assert.False(_authorizer.IsAllowed(actor, Update, Owned(SomeoneElseId)));
        Assert.False(_authorizer.IsAllowed(actor, Update));
    }

    [Theory]
    [InlineData(PermissionCatalog.AttendanceWarning.UpdateAny, AccessScope.Any)]
    [InlineData(PermissionCatalog.AttendanceWarning.UpdateOwn, AccessScope.Own)]
    [InlineData(PermissionCatalog.AttendanceWarning.ReadAny, AccessScope.None)]
    public void Scope_follows_the_widest_permission_held(string granted, AccessScope expected)
    {
        Assert.Equal(expected, _authorizer.ResolveScope(Actor(granted), Update));
    }

    private static GuildActor Actor(params string[] permissions)
    {
        return new GuildActor
        {
            DiscordUserId = MemberId,
            Permissions = PermissionCatalog.Expand(permissions),
        };
    }

    private static IOwnedByGuildMember Owned(ulong ownerDiscordUserId)
    {
        return new StubOwnedRecord(ownerDiscordUserId);
    }

    private sealed record StubOwnedRecord(ulong OwnerDiscordUserId) : IOwnedByGuildMember;
}
