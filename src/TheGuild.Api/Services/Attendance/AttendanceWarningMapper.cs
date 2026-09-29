using TheGuild.Api.Authorization;
using TheGuild.DataLayer.Models.Access;
using ApiAttendanceWarning = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarning;
using ApiAttendanceWarningType = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarningType;
using StoredAttendanceWarning = TheGuild.DataLayer.Models.Attendance.Warnings.AttendanceWarning;

namespace TheGuild.Api.Services.Attendance;

public static class AttendanceWarningMapper
{
    /// <summary>
    /// The private comment is dropped unless the actor may read it. Redacting during mapping rather
    /// than at each call site means a new endpoint cannot forget to do it.
    /// </summary>
    public static ApiAttendanceWarning ToModel(this StoredAttendanceWarning warning, GuildActor actor)
    {
        var mayReadPrivateComments = actor.Has(PermissionCatalog.AttendanceWarning.ReadPrivate);

        return new ApiAttendanceWarning
        {
            Id = warning.Id,
            DiscordServerId = warning.DiscordServerId,
            DiscordUserId = warning.DiscordUserId,
            Type = (ApiAttendanceWarningType)warning.Type,
            DateStart = warning.DateStart,
            DateEnd = warning.DateEnd,
            PublicComment = warning.PublicComment,
            PrivateComment = mayReadPrivateComments ? warning.PrivateComment : null,
            Created = warning.Created,
            Updated = warning.Updated,
        };
    }
}
