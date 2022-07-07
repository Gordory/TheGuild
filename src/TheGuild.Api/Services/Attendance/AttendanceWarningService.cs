using TheGuild.Api.Authorization;
using TheGuild.Api.Models.Attendance.Warnings;
using TheGuild.Api.Models.Users.Discord;

namespace TheGuild.Api.Services.Attendance;

public interface IAttendanceWarningService
{
    Task<AttendanceWarning> GetAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId);
    Task<ICollection<AttendanceWarning>> FindAsync(AuthorizedGuildUser authorizedUser, DateTime? guildName = null, DiscordUser? user = null);
    Task<AttendanceWarning> CreateAsync(AuthorizedGuildUser authorizedUser, AttendanceWarningCreateRequest attendanceWarningCreateRequest);
    Task<AttendanceWarning> UpdateAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId, AttendanceWarningUpdateRequest attendanceWarningUpdateRequest);
    Task<AttendanceWarning> DeleteAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId);
}

public class AttendanceWarningService : IAttendanceWarningService
{
    public Task<AttendanceWarning> GetAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<AttendanceWarning>> FindAsync(AuthorizedGuildUser authorizedUser, DateTime? guildName = null, DiscordUser? user = null)
    {
        throw new NotImplementedException();
    }

    public Task<AttendanceWarning> CreateAsync(AuthorizedGuildUser authorizedUser,
        AttendanceWarningCreateRequest attendanceWarningCreateRequest)
    {
        throw new NotImplementedException();
    }

    public Task<AttendanceWarning> UpdateAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId,
        AttendanceWarningUpdateRequest attendanceWarningUpdateRequest)
    {
        throw new NotImplementedException();
    }

    public Task<AttendanceWarning> DeleteAsync(AuthorizedGuildUser authorizedUser, Guid attendanceWarningId)
    {
        throw new NotImplementedException();
    }
}