namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// A record that belongs to one guild member, identified by their account here rather than by a
/// Discord id: Discord is one way into an account among several, and a person must keep their
/// records after switching providers.
/// </summary>
public interface IOwnedByGuildMember
{
    Guid OwnerUserId { get; }
}
