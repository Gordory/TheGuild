using TheGuild.Api.Models.Attendance.Warnings;

namespace TheGuild.Api.Services.Attendance;

public record AttendanceWarningCreateRequest
{
    /// <summary>
    /// The member this is about, named the only way a Discord bot can name anybody. The service turns
    /// it into an account, creating one if this person has never signed in.
    /// </summary>
    public ulong DiscordUserId { get; init; }

    public AttendanceWarningType Type { get; init; }

    public DateTime DateStart { get; init; }

    public DateTime? DateEnd { get; init; }

    public string? PublicComment { get; init; }

    public string? PrivateComment { get; init; }
}
