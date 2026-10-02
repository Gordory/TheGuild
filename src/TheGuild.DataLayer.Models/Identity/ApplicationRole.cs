using AspNetCore.Identity.Mongo.Model;

namespace TheGuild.DataLayer.Models.Identity;

/// <summary>
/// An application-wide role, such as platform support. Guild roles never live here: this is flat and
/// has no tenant dimension, while "officer" only means anything inside one guild. Those stay in
/// <see cref="Guilds.GuildRole"/> with their own permission grants.
/// </summary>
public class ApplicationRole : MongoRole<Guid>
{
}
