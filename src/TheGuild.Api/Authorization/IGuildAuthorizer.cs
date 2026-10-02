using System.Security.Claims;
using TheGuild.Api.Authentication.ApiKey;
using TheGuild.DataLayer.Models.Access;

namespace TheGuild.Api.Authorization;

public interface IGuildAuthorizer
{
    /// <summary>
    /// Whether the actor may apply <paramref name="permission"/> to <paramref name="resource"/>.
    /// Synchronous on purpose: the actor's permissions are already resolved and ownership is a
    /// property of the loaded resource, so there is nothing left to await.
    /// </summary>
    bool IsAllowed(GuildActor actor, PermissionPair permission, IOwnedByGuildMember? resource = null);

    /// <summary>
    /// The same decision where ownership has already been established by other means -- creating a
    /// record about someone, for instance, where comparing the two people does not need the record to
    /// exist first, and must not create an account as a side effect of being refused.
    /// </summary>
    bool IsAllowedFor(GuildActor actor, PermissionPair permission, bool isOwnRecord);

    AccessScope ResolveScope(GuildActor actor, PermissionPair permission);
}

public sealed class GuildAuthorizer : IGuildAuthorizer
{
    public bool IsAllowed(GuildActor actor, PermissionPair permission, IOwnedByGuildMember? resource = null)
    {
        // No resource means the request was never narrowed to the actor's own record, so holding only
        // the narrower permission cannot satisfy it.
        var isOwnRecord = resource is not null
                          && actor.UserId is not null
                          && resource.OwnerUserId == actor.UserId;

        return resource is null
            ? actor.Has(permission.Any)
            : IsAllowedFor(actor, permission, isOwnRecord);
    }

    public bool IsAllowedFor(GuildActor actor, PermissionPair permission, bool isOwnRecord)
    {
        return actor.Has(permission.Any) || (isOwnRecord && actor.Has(permission.Own));
    }

    public AccessScope ResolveScope(GuildActor actor, PermissionPair permission)
    {
        if (actor.Has(permission.Any))
        {
            return AccessScope.Any;
        }

        return actor.Has(permission.Own) ? AccessScope.Own : AccessScope.None;
    }
}

public interface IGuildActorAccessor
{
    Task<GuildActor> GetAsync(ulong discordServerId, ClaimsPrincipal principal);
}

public sealed class GuildActorAccessor : IGuildActorAccessor
{
    private readonly IGuildActorProvider _guildActorProvider;

    public GuildActorAccessor(IGuildActorProvider guildActorProvider)
    {
        _guildActorProvider = guildActorProvider;
    }

    public async Task<GuildActor> GetAsync(ulong discordServerId, ClaimsPrincipal principal)
    {
        var actingUser = principal.FindFirst(ApiKeyAuthenticationDefaults.ActingUserClaimType);

        // A service client that named no member is acting on its own behalf, and permissions belong
        // to members. Nothing to resolve means nothing granted, which is the safe answer.
        if (actingUser is null || !ulong.TryParse(actingUser.Value, out var actingDiscordUserId))
        {
            return new GuildActor { DiscordServerId = discordServerId };
        }

        return await _guildActorProvider.GetAsync(discordServerId, actingDiscordUserId);
    }
}
