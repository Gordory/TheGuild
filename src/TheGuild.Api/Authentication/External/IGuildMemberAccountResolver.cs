using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Identity;
using TheGuild.DataLayer.Models.Identity;

namespace TheGuild.Api.Authentication.External;

/// <summary>
/// Translates the only identifier a Discord bot knows into the account that identifies the person
/// here. Two modes on purpose: reading must not have the side effect of creating accounts, while
/// recording something about a member has to work even if that member never signed in.
/// </summary>
public interface IGuildMemberAccountResolver
{
    Task<Guid?> FindAsync(ulong discordUserId);

    Task<Guid> EnsureAsync(ulong discordUserId);
}

public sealed class GuildMemberAccountResolver : IGuildMemberAccountResolver
{
    // The same name the Discord authentication scheme writes as LoginProvider, so an account created
    // here is the one the person's first real sign-in finds.
    private const string Provider = DiscordAuthenticationDefaults.AuthenticationScheme;

    private readonly UserManager<ApplicationUser> _users;

    public GuildMemberAccountResolver(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public async Task<Guid?> FindAsync(ulong discordUserId)
    {
        var user = await _users.FindByLoginAsync(Provider, discordUserId.ToString());

        return user?.Id;
    }

    public async Task<Guid> EnsureAsync(ulong discordUserId)
    {
        var existing = await FindAsync(discordUserId);
        if (existing is not null)
        {
            return existing.Value;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"{Provider}:{discordUserId}",
        };

        var created = await _users.CreateAsync(user);

        if (!created.Succeeded)
        {
            // Two requests about the same member can reach this at once, and the unique user name is
            // what stops the second one. Losing that race means the account now exists.
            return await FindAsync(discordUserId)
                   ?? throw new InvalidOperationException(
                       $"Could not create an account for Discord user {discordUserId}: " +
                       string.Join("; ", created.Errors.Select(error => error.Description)));
        }

        var linked = await _users.AddLoginAsync(
            user,
            new UserLoginInfo(Provider, discordUserId.ToString(), Provider));

        if (!linked.Succeeded)
        {
            // An account with no login is unreachable forever, and would shadow the member's first
            // real sign-in, so refuse to leave one behind.
            await _users.DeleteAsync(user);

            throw new InvalidOperationException(
                $"Could not attach the Discord login for {discordUserId}: " +
                string.Join("; ", linked.Errors.Select(error => error.Description)));
        }

        return user.Id;
    }
}
