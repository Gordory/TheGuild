using AspNetCore.Identity.Mongo.Model;

namespace TheGuild.DataLayer.Models.Identity;

/// <summary>
/// One person's account. There is no password: every sign-in arrives through an external provider,
/// and several providers can point at the same account, so a person keeps their history after
/// switching from Discord to Battle.net or Google.
/// </summary>
public class ApplicationUser : MongoUser<Guid>
{
    /// <summary>
    /// What to show instead of an id, taken from the provider used to sign in and refreshed on every
    /// sign-in. A cached label, never an identifier: Discord lets people rename themselves, and two
    /// people can hold the same name at different times.
    /// </summary>
    public string? DisplayName { get; set; }
}
