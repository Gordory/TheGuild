using Microsoft.Extensions.Caching.Memory;
using TheGuild.Api.Authorization;
using TheGuild.Api.Tests.Fakes;
using TheGuild.DataLayer.Models.Access;
using TheGuild.DataLayer.Models.Guilds;
using Xunit;

namespace TheGuild.Api.Tests.Authorization;

public class GuildActorProviderTests
{
    private const ulong ServerId = 111;
    private const ulong MemberId = 222;
    private const ulong RaiderDiscordRoleId = 333;
    private const ulong OfficerDiscordRoleId = 444;

    [Fact]
    public async Task A_role_linked_to_a_discord_role_the_member_holds_grants_its_permissions()
    {
        var provider = Provider(
            Guild(Role("Raider", discordRoleIds: new[] { RaiderDiscordRoleId },
                grants: PermissionCatalog.AttendanceWarning.ReadOwn)),
            memberRoleIds: new[] { RaiderDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.True(actor.Has(PermissionCatalog.AttendanceWarning.ReadOwn));
    }

    [Fact]
    public async Task A_role_naming_the_member_outright_grants_without_any_discord_role()
    {
        var provider = Provider(
            Guild(Role("Treasurer", discordUserIds: new[] { MemberId },
                grants: PermissionCatalog.AttendanceWarning.ReadAny)),
            memberRoleIds: Array.Empty<ulong>());

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.True(actor.Has(PermissionCatalog.AttendanceWarning.ReadAny));
    }

    [Fact]
    public async Task Permissions_from_several_roles_are_combined()
    {
        var provider = Provider(
            Guild(
                Role("Raider", discordRoleIds: new[] { RaiderDiscordRoleId },
                    grants: PermissionCatalog.AttendanceWarning.ReadOwn),
                Role("Officer", discordRoleIds: new[] { OfficerDiscordRoleId },
                    grants: PermissionCatalog.AttendanceWarning.ReadPrivate)),
            memberRoleIds: new[] { RaiderDiscordRoleId, OfficerDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.True(actor.Has(PermissionCatalog.AttendanceWarning.ReadOwn));
        Assert.True(actor.Has(PermissionCatalog.AttendanceWarning.ReadPrivate));
    }

    [Fact]
    public async Task An_implied_permission_comes_with_the_one_that_implies_it()
    {
        var provider = Provider(
            Guild(Role("Officer", discordRoleIds: new[] { OfficerDiscordRoleId },
                grants: PermissionCatalog.AttendanceWarning.ReadAny)),
            memberRoleIds: new[] { OfficerDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.True(actor.Has(PermissionCatalog.AttendanceWarning.ReadOwn));
    }

    [Fact]
    public async Task A_member_matching_no_role_gets_nothing()
    {
        var provider = Provider(
            Guild(Role("Officer", discordRoleIds: new[] { OfficerDiscordRoleId },
                grants: PermissionCatalog.AttendanceWarning.ReadAny)),
            memberRoleIds: new[] { RaiderDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.Empty(actor.Permissions);
        Assert.Empty(actor.GrantedBy);
    }

    [Fact]
    public async Task An_unregistered_server_grants_nothing_rather_than_failing()
    {
        var provider = Provider(guild: null, memberRoleIds: new[] { OfficerDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.Empty(actor.Permissions);
        Assert.Equal(ServerId, actor.DiscordServerId);
    }

    [Fact]
    public async Task Only_roles_that_granted_something_are_reported_as_the_source()
    {
        var granting = Role("Officer", discordRoleIds: new[] { OfficerDiscordRoleId },
            grants: PermissionCatalog.AttendanceWarning.ReadAny);
        var empty = Role("Veteran", discordRoleIds: new[] { OfficerDiscordRoleId });

        var provider = Provider(Guild(granting, empty), memberRoleIds: new[] { OfficerDiscordRoleId });

        var actor = await provider.GetAsync(ServerId, MemberId);

        Assert.Equal(new[] { granting.Id }, actor.GrantedBy);
    }

    private static GuildActorProvider Provider(Guild? guild, ulong[] memberRoleIds)
    {
        return new GuildActorProvider(
            new FakeGuildRepository(guild),
            new FakeGuildMemberRolesReader(memberRoleIds),
            new MemoryCache(new MemoryCacheOptions()));
    }

    private static Guild Guild(params GuildRole[] roles)
    {
        return new Guild
        {
            Id = Guid.NewGuid(),
            DiscordServerId = ServerId,
            GuildRoles = roles,
        };
    }

    private static GuildRole Role(
        string name,
        ulong[]? discordRoleIds = null,
        ulong[]? discordUserIds = null,
        params string[] grants)
    {
        return new GuildRole
        {
            Id = Guid.NewGuid(),
            Name = name,
            DiscordRoleIds = discordRoleIds ?? Array.Empty<ulong>(),
            DiscordUserIds = discordUserIds ?? Array.Empty<ulong>(),
            Grants = grants.Select(grant => new PermissionGrant(grant)).ToArray(),
        };
    }
}
