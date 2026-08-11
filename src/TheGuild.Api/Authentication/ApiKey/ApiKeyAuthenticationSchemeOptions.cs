using Microsoft.AspNetCore.Authentication;

namespace TheGuild.Api.Authentication.ApiKey;

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKeyHeader { get; init; } = ApiKeyAuthenticationDefaults.ApiKeyHeader;

    public string ActingUserHeader { get; init; } = ApiKeyAuthenticationDefaults.ActingUserHeader;
}