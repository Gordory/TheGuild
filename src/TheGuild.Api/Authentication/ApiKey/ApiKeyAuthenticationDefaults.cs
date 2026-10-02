namespace TheGuild.Api.Authentication.ApiKey;

public static class ApiKeyAuthenticationDefaults
{
    /// <summary>
    /// Default value for <see cref="Microsoft.AspNetCore.Authentication.AuthenticationScheme.Name"/>.
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// Default value for <see cref="Microsoft.AspNetCore.Authentication.AuthenticationScheme.DisplayName"/>.
    /// </summary>
    public const string DisplayName = "ApiKey";

    /// <summary>
    /// Default value for <see cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions.ClaimsIssuer"/>.
    /// </summary>
    public const string Issuer = "TheGuild";

    /// <summary>
    /// Default header key used to retrieve API key value
    /// </summary>
    public const string ApiKeyHeader = "X-API-KEY";

    /// <summary>
    /// Default header key a service client uses to declare the guild member it acts on behalf of.
    /// The bot resolves the member from the Discord interaction, so the API trusts what it asserts.
    /// </summary>
    public const string ActingUserHeader = "X-Acting-Discord-User-Id";

    /// <summary>
    /// Claim carrying the Discord id of the member a service client acts on behalf of. Permission
    /// checks resolve against this member rather than against the service client itself.
    /// </summary>
    public const string ActingUserClaimType = "theguild:acting_discord_user_id";
}