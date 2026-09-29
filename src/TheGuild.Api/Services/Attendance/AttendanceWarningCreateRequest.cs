using TheGuild.Api.Models.Attendance.Warnings;
using TheGuild.DataLayer.Models.Access;

namespace TheGuild.Api.Services.Attendance;

public record AttendanceWarningCreateRequest : IOwnedByGuildMember
{
    public ulong DiscordUserId { get; init; }

    public AttendanceWarningType Type { get; init; }

    public DateTime DateStart { get; init; }

    public DateTime? DateEnd { get; init; }

    public string? PublicComment { get; init; }

    public string? PrivateComment { get; init; }

    // The warning does not exist yet, so the member it is about is what ownership means here.
    public ulong OwnerDiscordUserId => DiscordUserId;
}
