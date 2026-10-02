namespace TheGuild.Api.Authentication.External;

public static class ExternalLoginClaims
{
    /// <summary>
    /// Whether the provider considers the address it gave us verified. The Discord provider package
    /// maps no such claim of its own, so it is mapped from the raw payload in startup. Without it,
    /// an account could be linked on an address the provider never confirmed, which is the whole
    /// attack: register a Discord account on someone else's email, then claim their account here.
    /// </summary>
    public const string EmailVerified = "urn:theguild:email_verified";
}
