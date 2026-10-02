using TheGuild.Api.Authentication.External;

namespace TheGuild.Api.Tests.Fakes;

internal sealed class FakeGuildMemberAccountResolver : IGuildMemberAccountResolver
{
    private readonly Dictionary<ulong, Guid> _accounts;

    public FakeGuildMemberAccountResolver(params (ulong DiscordUserId, Guid UserId)[] accounts)
    {
        _accounts = accounts.ToDictionary(account => account.DiscordUserId, account => account.UserId);
    }

    /// <summary>
    /// How many accounts were brought into being. Lets a test state the invariant that matters: a
    /// request that gets refused leaves nobody with a new account.
    /// </summary>
    public int Created { get; private set; }

    public Task<Guid?> FindAsync(ulong discordUserId)
    {
        return Task.FromResult(_accounts.TryGetValue(discordUserId, out var userId) ? userId : (Guid?)null);
    }

    public Task<Guid> EnsureAsync(ulong discordUserId)
    {
        if (_accounts.TryGetValue(discordUserId, out var existing))
        {
            return Task.FromResult(existing);
        }

        var created = Guid.NewGuid();
        _accounts[discordUserId] = created;
        Created++;

        return Task.FromResult(created);
    }
}
