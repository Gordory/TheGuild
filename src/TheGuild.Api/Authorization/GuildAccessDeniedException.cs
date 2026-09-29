namespace TheGuild.Api.Authorization;

/// <summary>
/// Carries the permission that was missing. The bot refuses a slash command with a reason a member
/// can act on -- "you need X" -- and it can only do that if the refusal says which one.
/// </summary>
public sealed class GuildAccessDeniedException : Exception
{
    public GuildAccessDeniedException(string missingPermissionId)
        : base($"The acting member is missing the permission {missingPermissionId}")
    {
        MissingPermissionId = missingPermissionId;
    }

    public string MissingPermissionId { get; }
}
