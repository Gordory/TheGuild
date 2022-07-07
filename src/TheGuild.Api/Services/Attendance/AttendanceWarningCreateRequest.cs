using TheGuild.Api.Models.Attendance.Warnings;

namespace TheGuild.Api.Services.Attendance;

public record AttendanceWarningCreateRequest
{
    public long DiscordUserId { get; init; }

    public AttendanceWarningType Type { get; init; }

    public DateTime DateStart { get; init; }

    public DateTime? DateEnd { get; init; }

    public string? PublicComment { get; init; }

    public string? PrivateComment { get; init; }
}