using System.Security.Claims;

namespace TheGuild.Api.Authorization;

public interface IAuthorizedGuildUserProvider
{
    Task<AuthorizedGuildUser> GetAsync(ulong serverId, ClaimsPrincipal claimsPrincipal);
}

public class AuthorizedGuildUserProvider : IAuthorizedGuildUserProvider
{
    public Task<AuthorizedGuildUser> GetAsync(ulong serverId, ClaimsPrincipal claimsPrincipal)
    {
        throw new NotImplementedException();
    }
}