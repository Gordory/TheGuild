using TheGuild.Api.Models.Attendance.Warnings;

namespace TheGuild.Api.Models.Guild;

public record GuildPermissions
{
    public AttendanceWarningPermissions AttendanceWarningPermissions { get; init; }
}