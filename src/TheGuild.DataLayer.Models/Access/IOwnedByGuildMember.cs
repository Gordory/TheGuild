namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// A record that belongs to one guild member. The distinction the old flags drew between acting on
/// your own record and on someone else's lives here instead: the permission names a verb, and
/// ownership of the loaded resource decides which of the pair has to be held.
/// </summary>
public interface IOwnedByGuildMember
{
    ulong OwnerDiscordUserId { get; }
}
