using Microsoft.AspNetCore.Identity;

namespace TheGuild.DataLayer.Models.Identity;

public static class ApplicationIdentityOptions
{
    /// <summary>
    /// Shared by the host and by the tests on purpose: a test configured differently from production
    /// proves less than it appears to.
    /// </summary>
    public static void Configure(IdentityOptions options)
    {
        // A user name here is derived from the provider identity, never chosen by a person, so it
        // carries the provider's separator. The default character set exists to keep human-chosen
        // names safe in URLs, which is not what these are.
        options.User.AllowedUserNameCharacters += ":";

        // Accounts created for someone who has not signed in yet have no address at all, and several
        // of them must be able to coexist.
        options.User.RequireUniqueEmail = false;
    }
}
