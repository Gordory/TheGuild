using System.Security.Claims;
using System.Text.Encodings.Web;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TheGuild.DataLayer.Authentication;
using TheGuild.DataLayer.Models.Authentication;

namespace TheGuild.Api.Authentication.ApiKey;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string DiscordIssuer = "Discord";

    private readonly IApiKeyBindingRepository _apiKeyBindingRepository;

    public ApiKeyAuthenticationHandler(
        IApiKeyBindingRepository apiKeyBindingRepository,
        IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _apiKeyBindingRepository = apiKeyBindingRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var apiKeyHeader = Context.Request.Headers[Options.ApiKeyHeader];
        if (!Guid.TryParse(apiKeyHeader, out var apiKeyValue))
        {
            return AuthenticateResult.Fail($"Invalid {Options.ApiKeyHeader} header format");
        }

        var apiKeyBinding = await _apiKeyBindingRepository.FindAsync(apiKeyValue);

        if (apiKeyBinding is not { Enabled: true })
        {
            return AuthenticateResult.Fail($"Invalid {Options.ApiKeyHeader} header");
        }

        if (apiKeyBinding is DiscordBotApiKeyBinding discordBotApiKeyBinding)
        {
            if (!TryReadActingDiscordUserId(out var actingDiscordUserId))
            {
                return AuthenticateResult.Fail($"Invalid {Options.ActingUserHeader} header format");
            }

            var nameIdentifierClaim = new Claim(
                ClaimTypes.NameIdentifier,
                discordBotApiKeyBinding.DiscordBotClientId.ToString(),
                valueType: null,
                issuer: ApiKeyAuthenticationDefaults.Issuer,
                originalIssuer: DiscordAuthenticationDefaults.Issuer);

            var nameClaim = new Claim(
                ClaimTypes.Name,
                discordBotApiKeyBinding.ServiceName,
                valueType: null,
                ApiKeyAuthenticationDefaults.Issuer);

            var claims = new List<Claim>
            {
                nameIdentifierClaim,
                nameClaim,
            };

            // Absent when the bot acts on its own behalf. Permission checks that need a member
            // reject such a request themselves; authentication has nothing to object to here.
            if (actingDiscordUserId != null)
            {
                claims.Add(new Claim(
                    ApiKeyAuthenticationDefaults.ActingUserClaimType,
                    actingDiscordUserId.Value.ToString(),
                    valueType: null,
                    issuer: ApiKeyAuthenticationDefaults.Issuer,
                    originalIssuer: DiscordAuthenticationDefaults.Issuer));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        throw new ArgumentOutOfRangeException($"Unknown ApiKey binding type");
    }

    private bool TryReadActingDiscordUserId(out ulong? actingDiscordUserId)
    {
        actingDiscordUserId = null;

        var header = Context.Request.Headers[Options.ActingUserHeader];
        if (header.Count == 0)
        {
            return true;
        }

        if (!ulong.TryParse(header, out var parsed))
        {
            return false;
        }

        actingDiscordUserId = parsed;
        return true;
    }
}