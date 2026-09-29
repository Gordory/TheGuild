using TheGuild.DataLayer.Guilds;
using TheGuild.DataLayer.Models.Guilds;

namespace TheGuild.Api.Tests.Fakes;

/// <summary>
/// Hand-written rather than mocked: only one method is ever exercised, and the rest throwing makes
/// an accidental new dependency on them loud instead of silently returning a default.
/// </summary>
internal sealed class FakeGuildRepository : IGuildRepository
{
    private readonly Guild? _guild;

    public FakeGuildRepository(Guild? guild) => _guild = guild;

    public Task<Guild?> FindByDiscordServerId(ulong discordServerId)
    {
        return Task.FromResult(_guild?.DiscordServerId == discordServerId ? _guild : null);
    }

    public Task<Guild?> FindAsync(Guid id) => throw new NotSupportedException();

    public Task<ICollection<Guild>> GetAllAsync() => throw new NotSupportedException();

    public Task<Guild> CreateAsync(Guild entity) => throw new NotSupportedException();

    public Task<Guild> UpdateAsync(Guild entity) => throw new NotSupportedException();

    public Task DeleteAsync(Guid id) => throw new NotSupportedException();
}
