using TheGuild.Api.Models.Attendance.Warnings;

namespace TheGuild.Api.Models.Guilds;

public record GuildPermissions
{
    public AttendanceWarningPermissions AttendanceWarningPermissions { get; init; }
}