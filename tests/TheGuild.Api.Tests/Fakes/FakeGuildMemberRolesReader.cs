using TheGuild.Api.Authorization;

namespace TheGuild.Api.Tests.Fakes;

internal sealed class FakeGuildMemberRolesReader : IGuildMemberRolesReader
{
    private readonly ulong[] _roleIds;

    public FakeGuildMemberRolesReader(params ulong[] roleIds) => _roleIds = roleIds;

    public Task<ulong[]> GetRoleIdsAsync(ulong discordServerId, ulong discordUserId)
    {
        return Task.FromResult(_roleIds);
    }
}
