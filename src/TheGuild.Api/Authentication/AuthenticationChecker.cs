using System.Security.Claims;

namespace TheGuild.Api.Authentication;

public interface IAuthenticationChecker
{
    Task EnsureUserIsAuthenticatedAsync(ClaimsPrincipal claimsPrincipal);
}

public class AuthenticationChecker : IAuthenticationChecker
{
    public Task EnsureUserIsAuthenticatedAsync(ClaimsPrincipal claimsPrincipal)
    {
        throw new NotImplementedException();
    }
}