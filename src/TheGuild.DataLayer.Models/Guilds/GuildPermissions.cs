using TheGuild.DataLayer.Models.Attendance.Warnings;

namespace TheGuild.DataLayer.Models.Guilds;

public record GuildPermissions
{
    public AttendanceWarningPermissions AttendanceWarningPermissions { get; init; }
}