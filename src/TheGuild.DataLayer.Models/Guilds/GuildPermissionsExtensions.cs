namespace TheGuild.DataLayer.Models.Guilds;

public static class GuildPermissionsExtensions
{
    public static GuildPermissions Merge(this IEnumerable<GuildPermissions> guildPermissions)
    {
        return guildPermissions.Aggregate(
            new GuildPermissions(),
            (current, other) => current.Merge(other));
    }

    public static GuildPermissions Merge(this GuildPermissions first, GuildPermissions second)
    {
        return new GuildPermissions
        {
            AttendanceWarningPermissions = first.AttendanceWarningPermissions | second.AttendanceWarningPermissions,
        };
    }
}